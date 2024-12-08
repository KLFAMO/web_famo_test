import socket

def send(ip='192.168.3.54', port=23, mes='DLOCK:ON ?'):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect((ip,port))
    s.recv(1024)
    s.send(mes.encode('UTF-8'))
    ans = s.recv(1024)
    s.close()
    return ans.decode('UTF-8')
