import sys
import socket
import selectors
import types
import array
import traceback

import brains

HOST = "127.0.0.1"
PORT = 65432

print("Running socket manager.")

class SocketManager:

    def service_connection(self, key, mask):
        try:
            sock = key.fileobj
            sockID = key.data
            if mask & selectors.EVENT_READ:
                recv_data = sock.recv(1024)
                if recv_data:
                    doubles = array.array('d', recv_data)
                    handler = self.ioHandlers[sockID]
                    if isinstance(handler, brains.SystemHandler):
                        handler.handle_input(doubles, self.ioHandlers)
                        if brains.SystemHandler.do_interbreed:
                            brain_handlers = []
                            for handler_scan in self.ioHandlers.values:
                                if isinstance(handler_scan, brains.Brain):
                                    brain_handlers.append(handler_scan)
                            self.next_generation = brains.interbreed_by_fitness(brain_handlers)
                            brains.SystemHandler.do_interbreed = False
                        if brains.SystemHandler.apply_next_generation:
                            new_list = [handler].extend(self.next_generation)
                            self.ioHandlers = dict()
                            for next_handler in new_list:
                                self.ioHandlers[next_handler.socketID] = next_handler
                            brains.SystemHandler.apply_next_generation = False
                    else:
                        #print(f"Requesting to handle input: {sockID}")
                        handler.handle_input(doubles)
                else:
                    print(f"Closing connection to {sockID}")
                    self.sel.unregister(sock)
                    sock.close()
                    self.ioHandlers.pop(sockID)
                    if(self.toDispatch.keys().__contains__(sockID)):
                        self.toDispatch.pop(sockID)
            if mask & selectors.EVENT_WRITE:
                if(self.toDispatch.keys().__contains__(sockID)):
                    #print(f"Send on {sockID}: {self.toDispatch[sockID]}")
                    #print(str(self.toDispatch[sockID]))
                    sent = sock.send(bytes(self.toDispatch[sockID]))
                    self.toDispatch.pop(sockID)
        except Exception as e:
            exc_type, exc_value, exc_traceback = sys.exc_info()
            print(f"Exception caught while servicing connection ID {sockID}.\n{exc_type}: ({exc_value}) {traceback.extract_tb(exc_traceback)}")
            print(self.toDispatch.keys())

    def accept_wrapper(self, sock):
        conn, addr = sock.accept()
        print(f"Accepted connection from {addr}")
        conn.setblocking(False)
        events = selectors.EVENT_READ | selectors.EVENT_WRITE
        self.sel.register(conn, events, data=self.nextConnectionID)
        if self.nextConnectionID == 0:
            self.ioHandlers[self.nextConnectionID] = brains.SystemHandler(self.nextConnectionID)
            print("SYSTEM_SOCKET_ASSIGNED")
        else:
            self.ioHandlers[self.nextConnectionID] = brains.SimpleFeedForwardDense(self.nextConnectionID)
        print(f"Registered connection as: {self.nextConnectionID}")
        self.nextConnectionID = self.nextConnectionID + 1

    def __run_socket_selector__(self):
        try:
            while True:
                events = self.sel.select(timeout=None)
                for key, mask in events:
                    if key.data is None:
                        self.accept_wrapper(key.fileobj)
                    else:
                        self.service_connection(key, mask)
                for ioHandlerKey in self.ioHandlers.keys():
                    output = self.ioHandlers[ioHandlerKey].get_output()
                    if output != None:
                        self.toDispatch[ioHandlerKey] = output
                    self.ioHandlers[ioHandlerKey].nextOutput = None
        except KeyboardInterrupt:
            print("Caught keyboard interrupt, exiting")
        finally:
            self.sel.close()

    def __init__(self):
        self.nextConnectionID = 0
        self.toDispatch = dict()
        self.ioHandlers = dict()
        self.next_generation = []
        self.sel = selectors.DefaultSelector()
        self.serSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.serSocket.bind((HOST, PORT))
        self.serSocket.listen()
        print(f"Listening on {(HOST, PORT)}")
        print("SOCKETS_OPEN_AND_READY")
        self.serSocket.setblocking(False)
        self.sel.register(self.serSocket, selectors.EVENT_READ, data=None)
        self.__run_socket_selector__()

manager = SocketManager()