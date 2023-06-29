import sys
import socket
import selectors
import types
import array

import brains

HOST = "127.0.0.1"
PORT = 65432

print("Running socket manager.")

class SocketManager:

    def service_connection(self, key, mask):
        sock = key.fileobj
        sockID = key.data
        if mask & selectors.EVENT_READ:
            recv_data = sock.recv(1024)
            if recv_data:
                #print(f"Received! {sockID}")
                doubles = array.array('d', recv_data)
                self.ioHandlers[sockID].handleInput(doubles)
            else:
                print(f"Closing connection to {sockID}")
                sel.unregister(sock)
                sock.close()
                self.ioHandlers.pop(sockID)
                self.toDispatch.pop(sockID)
        if mask & selectors.EVENT_WRITE:
            if(self.toDispatch.keys().__contains__(sockID)):
                #print(f"Send on {sockID}: {self.toDispatch[sockID]}")
                sent = sock.send(bytes(self.toDispatch[sockID]))
                self.toDispatch.pop(sockID)

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
            self.ioHandlers[self.nextConnectionID] = brains.IOHandler(self.nextConnectionID)
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
                    output = self.ioHandlers[ioHandlerKey].getOutput()
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