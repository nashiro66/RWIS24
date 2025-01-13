using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScannerTestScript : MonoBehaviour
{
	public GameObject ScannedItemPrefab;
	public bool _connect = false;

	public string _searchingDeviceName = "android";

	// defined here https://www.bluetooth.com/specifications/assigned-numbers/
    public string UUID = "100b615f-f517-43a3-81fa-d06b9e169839";
	
	private float _timeout;
	private float _startScanTimeout = 10f;
	private float _startScanDelay = 0.5f;
	private bool _startScan = true;
	private Dictionary<string, ScannedItemScript> _scannedItems;
    private string _deviceAddress;
    private bool _connected = false;
	public Transform _scrollViewContent;
	float yPos = 0f;
	public void OnStopScanning()
	{
		BluetoothLEHardwareInterface.Log ("**************** stopping");
		BluetoothLEHardwareInterface.StopScan ();
	}
	string FullUUID(string uuid)
    {
        string fullUUID = uuid;
        if (fullUUID.Length == 4)
            fullUUID = "0000" + uuid + "-0000-1000-8000-00805f9b34fb";

        return fullUUID;
    }
	bool IsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
            uuid1 = FullUUID(uuid1);
        if (uuid2.Length == 4)
            uuid2 = FullUUID(uuid2);

        return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
    }
	
	void Start ()
	{
		BluetoothLEHardwareInterface.Log ("Start");
		_scannedItems = new Dictionary<string, ScannedItemScript> ();

		BluetoothLEHardwareInterface.Initialize (true, false, () => {

			_timeout = _startScanDelay;
		}, 
		(error) => {
			
			BluetoothLEHardwareInterface.Log ("Error: " + error);

			if (error.Contains ("Bluetooth LE Not Enabled"))
				BluetoothLEHardwareInterface.BluetoothEnable (true);
		});
	}
	
	void Update ()
	{
		if (_timeout > 0f)
		{
			_timeout -= Time.deltaTime;
			if (_timeout <= 0f)
			{
				if (_startScan)
				{
					_startScan = false;
					_timeout = _startScanTimeout;
					
					
					
					BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, null, (address, name, rssi, bytes) => {
					if(name != "No Name")
					{
						BluetoothLEHardwareInterface.Log ("item scanned: " + address);
						if (_scannedItems.ContainsKey (address))
						{
							var scannedItem = _scannedItems[address];
							scannedItem.TextRSSIValue.text = rssi.ToString ();
							BluetoothLEHardwareInterface.Log ("already in list " + rssi.ToString ());
						}
						else
						{
							BluetoothLEHardwareInterface.Log ("item new: " + address);
							var newItem = Instantiate (ScannedItemPrefab);
							if (newItem != null)
							{
								BluetoothLEHardwareInterface.Log ("item created: " + address);
								newItem.transform.SetParent(_scrollViewContent, false);
								newItem.transform.SetPositionAndRotation(new Vector3(0.0f, yPos, 0.0f), new Quaternion(0.0f, 0.0f, 0.0f,0.0f));
								yPos-=100.0f;
								
								newItem.transform.localScale = Vector3.one;
								var scannedItem = newItem.GetComponent<ScannedItemScript> ();
								if (scannedItem != null)
								{
									BluetoothLEHardwareInterface.Log ("item set: " + address);
									scannedItem.TextAddressValue.text = address;
									scannedItem.TextNameValue.text = name;
									scannedItem.TextRSSIValue.text = rssi.ToString ();
									_scannedItems[address] = scannedItem;
									
								}
							}
						}
					}
					if (name.Contains(_searchingDeviceName))
					{
						_deviceAddress = address;
					}
					}, true);
					
				}
				else
				{
					BluetoothLEHardwareInterface.StopScan ();
					_startScan = true;
					_timeout = _startScanDelay;
				}

				if(_connect)
				{                 
					BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceAddress, null, null, (address, ServiceUUID, characteristicUUID ) =>
                        {
                            BluetoothLEHardwareInterface.StopScan();
                            if (IsEqual(UUID, ServiceUUID))
                            {
								// 接続先のbluetoothにジャイロのデータを書き込む
								byte[] data = { 3 };
								BluetoothLEHardwareInterface.WriteCharacteristic(_deviceAddress, ServiceUUID, UUID, data, data.Length, true, (characteristicUUID) =>
								{
									BluetoothLEHardwareInterface.Log("Write Succeeded");
								});
								BluetoothLEHardwareInterface.ReadCharacteristic(_deviceAddress, ServiceUUID, UUID, (characteristic, bytes) =>
								{
									BluetoothLEHardwareInterface.Log("Read");
								});
								
                            }
                        }, (disconnectDeviceAddress) =>
                        {
                            _connected = false;
                        });
				}
			}
		}
	}
	void Select ()
	{
	}
}
