using System;
using System.Collections.Concurrent;
using ShimmerAPI;

namespace ShimmerLVWrapper
{
    public class ShimmerBridge
    {
        private ShimmerLogAndStreamSystemSerialPort _device;
        private readonly ConcurrentQueue<ObjectCluster> _buffer =
            new ConcurrentQueue<ObjectCluster>();

        private bool _streaming = false;
        private bool _connected = false;

        // -----------------------------------------------------------------
        // 1. Constructor (LabVIEW will call this)
        // -----------------------------------------------------------------
        public ShimmerBridge(string comPort)
        {
            try
            {
                // Directly create the device
                _device = new ShimmerLogAndStreamSystemSerialPort("Shimmer3", comPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Constructor error: " + ex.Message);
                throw;  // propagate to LabVIEW
            }
        }

        // -----------------------------------------------------------------
        // 2. Connect
        // -----------------------------------------------------------------
        public bool Connect()
        {
            try
            {
                if (_device == null) return false;

                _device.StartConnectThread();
                _connected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connect error: " + ex.Message);
                return false;
            }
        }

        // -----------------------------------------------------------------
        // 3. Start Streaming
        // -----------------------------------------------------------------
        public bool StartStreaming()
        {
            try
            {
                if (_device == null || !_connected) return false;

                // clear old data
                while (_buffer.TryDequeue(out _)) { }

                _device.UICallback += HandleEvent;
                _device.StartStreaming();
                _streaming = true;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("StartStreaming error: " + ex.Message);
                return false;
            }
        }

        // -----------------------------------------------------------------
        // 4. Stop Streaming
        // -----------------------------------------------------------------
        public bool StopStreaming()
        {
            try
            {
                if (_device == null) return false;

                _streaming = false;
                _device.StopStreaming();
                _device.UICallback -= HandleEvent;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("StopStreaming error: " + ex.Message);
                return false;
            }
        }

        // -----------------------------------------------------------------
        // 5. Disconnect
        // -----------------------------------------------------------------
        public bool Disconnect()
        {
            try
            {
                if (_device == null) return false;

                _streaming = false;
                _connected = false;

                _device.Disconnect();
                _device = null;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconnect error: " + ex.Message);
                return false;
            }
        }

        // -----------------------------------------------------------------
        // INTERNAL EVENT HANDLER
        // -----------------------------------------------------------------
        public void HandleEvent(object sender, EventArgs args)
        {
            CustomEventArgs eventArgs = (CustomEventArgs)args;
            int indicator = eventArgs.getIndicator();

            switch (indicator)
            {
                case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_DATA_PACKET:
                    if (eventArgs.getObject() is ObjectCluster oc)
                        _buffer.Enqueue(oc);
                    break;

                case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_STATE_CHANGE:
                    Console.WriteLine("State changed: " + eventArgs.getObject());
                    break;

                case (int)ShimmerBluetooth.ShimmerIdentifier.MSG_IDENTIFIER_NOTIFICATION_MESSAGE:
                    Console.WriteLine("Notification: " + eventArgs.getObject());
                    break;
            }
        }

        // -----------------------------------------------------------------
        // 6A. Queue depth
        // -----------------------------------------------------------------
        public int GetBufferedCount()
        {
            return _buffer.Count;
        }

        // -----------------------------------------------------------------
        // 6B. Dequeue → double[]
        // -----------------------------------------------------------------
        public bool TryDequeueDataArray(out double[] dataArray)
        {
            dataArray = null;

            if (!_streaming) return false;

            if (_buffer.TryDequeue(out var oc))
            {
                dataArray = oc?.GetData().ToArray();
                return dataArray != null;
            }

            return false;
        }

        // -----------------------------------------------------------------
        // 6C. Peek → double[]
        // -----------------------------------------------------------------
        public bool TryPeekDataArray(out double[] dataArray)
        {
            dataArray = null;

            if (!_streaming) return false;

            if (_buffer.TryPeek(out var oc))
            {
                dataArray = oc?.GetData().ToArray();
                return dataArray != null;
            }

            return false;
        }
    }
}
