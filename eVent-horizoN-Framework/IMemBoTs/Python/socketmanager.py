import sys
import socket
import selectors
import types
import array
import traceback

import brains #brains.py is imported with the neural model classes

#Define socket port and loopback address
HOST = "127.0.0.1"
PORT = 65432

print("Running socket manager.")

#The SocketManager class handles the Python-side socket bridge
class SocketManager:
    #Service a single socket connection defined by the key
    #The mask defines the type of socket event being handled
    def service_connection(self, key, mask):
        try:
            sock = key.fileobj
            sockID = key.data
            if mask & selectors.EVENT_READ: #When reading data from socket
                recv_data = sock.recv(1024) #Data is received
                if recv_data: #If data is found
                    doubles = array.array('d', recv_data) #Extract double precision values from received data
                    handler = self.ioHandlers[sockID] #Select socket handler object
                    if isinstance(handler, brains.SystemHandler): #If the handler is the system handler
                        handler.handle_input(doubles, self.ioHandlers.values()) #Send to handle new input
                        if brains.SystemHandler.do_interbreed: #If the system handler requested an interbreed event
                            print("Doing interbreed...")
                            brain_handlers = []
                            for handler_scan in self.ioHandlers.values(): #Collate all active brains in a list
                                if isinstance(handler_scan, brains.Brain):
                                    brain_handlers.append(handler_scan)
                            print(f"Found {len(brain_handlers)} brains.")
                            #Perform fitness interbreed
                            self.next_generation = brains.interbreed_by_fitness(brain_handlers, brains.SystemHandler.interbreed_rand_proportion, brains.SystemHandler.interbreed_param_uncertainty)
                            print("Interbreed complete.")
                            brains.SystemHandler.do_interbreed = False
                        if brains.SystemHandler.apply_next_generation: #If the system handler requests that the next generation be applied
                            print(f"Applying next generation of {len(self.next_generation)} brains.")
                            new_list = [handler] #Add the system handler to a new handler list
                            new_list.extend(self.next_generation) #Extend the list to include the next generation
                            newIOHandlers = dict()
                            for next_handler in new_list: #For each of the new handlers
                                next_handler.nextOutput = self.ioHandlers[next_handler.socketID].get_output() #Carry over any waiting output
                                newIOHandlers[next_handler.socketID] = next_handler #Add the new handler to a dict with the equivalent socket ID
                            self.ioHandlers = newIOHandlers #Reassign new handlers to overwrite old handlers
                            brains.SystemHandler.apply_next_generation = False
                            print("Generation applied.")
                        handler.nextOutput = array.array('d', [float(doubles[0])] * 128) #Set data for system handler return codes
                        print(f"SystemHandler responds: {handler.nextOutput[0]}")
                    else: #If it is not the system handler
                        handler.handle_input(doubles) #Simply pass action onto the handler object
                else: #If no data is received
                    print(f"Closing connection to {sockID}")
                    self.sel.unregister(sock)
                    sock.close() #Close the socket connection
                    self.ioHandlers.pop(sockID) #Remove its handler from the list
                    if(self.toDispatch.keys().__contains__(sockID)):
                        self.toDispatch.pop(sockID) #Remove any data queued to dispatch over the closed socket
            if mask & selectors.EVENT_WRITE: #When writing data to the socket
                if(self.toDispatch.keys().__contains__(sockID)): #Check if data is queued
                    sent = sock.send(bytes(self.toDispatch[sockID])) #Send the data
                    self.toDispatch.pop(sockID) #Remove the queued dispatch
        except Exception as e: #Exceptions are monitored for and printed back to the output stream
            exc_type, exc_value, exc_traceback = sys.exc_info()
            print(f"Exception caught while servicing connection ID {sockID}.\n{exc_type}: ({exc_value}) {traceback.extract_tb(exc_traceback)}")
            print(f"Current IO handlers: {self.ioHandlers.keys()}")
            print(f"Current sockets to dispatch for: {self.toDispatch.keys()}")

    def accept_wrapper(self, sock): #Method to accept an incoming socket connection
        conn, addr = sock.accept() #Socket connection is accepted
        print(f"Accepted connection from {addr}")
        conn.setblocking(False) #Connection should not block
        events = selectors.EVENT_READ | selectors.EVENT_WRITE #Read and write events are set
        self.sel.register(conn, events, data=self.nextConnectionID) #Register the new connection with the selector
        if self.nextConnectionID == 0: #If it is the first connection opened
            self.ioHandlers[self.nextConnectionID] = brains.SystemHandler(self.nextConnectionID) #Assign it the system socket connection
            print("SYSTEM_SOCKET_ASSIGNED") #Print system socket acknowledgement to the output
        else: #Otherwise, create a connection handler that incorporates a neural controller
            #To change the architecture under consideration, uncomment the correct model type
            self.ioHandlers[self.nextConnectionID] = brains.IterativeMemoryTransformer(self.nextConnectionID)
        #    self.ioHandlers[self.nextConnectionID] = brains.IMemBoT2(self.nextConnectionID)
        #    self.ioHandlers[self.nextConnectionID] = brains.SimpleFeedForwardDense(self.nextConnectionID)
        #    self.ioHandlers[self.nextConnectionID] = brains.ChimericMemBoT(self.nextConnectionID)
        print(f"Registered connection as: {self.nextConnectionID}")
        self.nextConnectionID = self.nextConnectionID + 1 #Increment the next connection ID

    def __run_socket_selector__(self): #Define the main Python socket response loop
        try:
            while True: #Run loop unless manually exited
                events = self.sel.select(timeout=None) #Get the event list from the socket selector
                for key, mask in events: #Get key and mask from the current event
                    if key.data is None: #If there is no data
                        self.accept_wrapper(key.fileobj) #Accept a new connection
                    else: #If there is data
                        self.service_connection(key, mask) #Service the existing connection
                for ioHandlerKey in self.ioHandlers.keys(): #For each IO handler
                    output = self.ioHandlers[ioHandlerKey].get_output() #Check if there is a new output to be sent over the socket
                    if output != None: #If there is an output
                        self.toDispatch[ioHandlerKey] = output #Add the output to the dispatch queue for that connection
                    self.ioHandlers[ioHandlerKey].nextOutput = None #Clear the IO handler's internal output buffer
        except KeyboardInterrupt: #Exit loop on interrupt
            print("Caught keyboard interrupt, exiting")
        finally:
            self.sel.close() #Close the selector

    def __init__(self): #Initialize the socket manager
        self.nextConnectionID = 0 #Connection IDs start at zero
        self.toDispatch = dict() #Socket ID indexed dict of what data to dispatch
        self.ioHandlers = dict() #Socket ID indexed dict of individual connection handlers
        self.next_generation = [] #List of the next generation as a result of the breeding process
        self.sel = selectors.DefaultSelector() #Create a socket selecter
        self.serSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM) #Create socket listener
        self.serSocket.bind((HOST, PORT)) #Bind socket to host and port
        self.serSocket.listen() #Listen to socket
        print(f"Listening on {(HOST, PORT)}")
        print("SOCKETS_OPEN_AND_READY")
        self.serSocket.setblocking(False) #Socket should not be blocking
        self.sel.register(self.serSocket, selectors.EVENT_READ, data=None) #Register socket with selector
        self.__run_socket_selector__() #Run the main loop

manager = SocketManager() #Create an instance of the class and start the script running