from emokit.emotiv import Emotiv
import platform
if platform.system() == "Windows":
    import socket
import gevent

if __name__ == "__main__":
  headset = Emotiv()    
  gevent.spawn(headset.setup)
  gevent.sleep(0)
  try:
    while True:
      packet = headset.dequeue()
      print packet.gyro_x, packet.gyro_y
      gevent.sleep(0)
  except KeyboardInterrupt:
    headset.close()
  finally:
    headset.close()