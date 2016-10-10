import socket
import thread
import struct
from enum import Enum

class RTConnectionStatus(Enum):
	Disconnected = 1
	Connecting = 2
	Connected = 3

class RTNetClient:
	buffer_size = 512

	def __init__(self):
		self.connection_status = RTConnectionStatus.Disconnected
		self.ip = ""
		self.port = 4434

	def __init__(self, ip, port):
		self.connection_status = RTConnectionStatus.Disconnected
		self.connect(ip, port)

	def __internal_receive(self):
		while(1):
			data, server = s.recvfrom(buffer_size)
			print data

	def connect(self, ip, port):
		self.connection_status = RTConnectionStatus.Connecting
		self.ip = ip
		self.port = port
		try:
			s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
		except socket.error:
			print 'Failed to create socket'

		buffer = (17, 19, 2)
		packer = struct.Struct('I I I')
		packed_data = packer.pack(*buffer)
		s.sendto(packed_data, (ip, port))
		thread.start_new_thread(self.__internal_receive, ("Receive Thread", 2))

	def send(self, data):
		s.sendto(data, (ip, port))