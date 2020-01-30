using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlubbFish.Utils;
using BlubbFish.Utils.IoT.Bots;
using BlubbFish.Utils.IoT.Connector;
using BlubbFish.Utils.IoT.Events;
using LitJson;

namespace Fraunhofer.Fit.IoT.TTN.Bridge {
  class Program : ABot {
    private ADataBackend ttnt;
    private ADataBackend ttns;
    private ADataBackend mqtt;
    private InIReader settings;
    private String tSBatt;
    private String tSLat;
    private String tSLon;
    private String tSHeight;
    private String tSSat;

    static void Main(String[] _1) => new Program();

    Program() {
      InIReader.SetSearchPath(new List<String>() { "/etc/ttnbridge", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ttnbridge" });
      if (!InIReader.ConfigExist("settings")) {
        Helper.WriteError("No settings.ini found. Abord!");
        return;
      }
      this.settings = InIReader.GetInstance("settings");
      this.logger.SetPath(this.settings.GetValue("logging", "path"));

      this.Connect();
      this.Attach();
      this.WaitForShutdown();
      this.Dispose();
    }

    private void Connect() {
      if (this.settings.GetSections(false).Contains("tracker")) {
        this.ttnt = (ADataBackend)ABackend.GetInstance(this.settings.GetSection("tracker"), ABackend.BackendType.Data);
      }
      if (this.settings.GetSections(false).Contains("sensors")) {
        this.ttns = (ADataBackend)ABackend.GetInstance(this.settings.GetSection("sensors"), ABackend.BackendType.Data);
      }
      this.mqtt = (ADataBackend)ABackend.GetInstance(this.settings.GetSection("to"), ABackend.BackendType.Data);
    }

    private void Attach() {
      if (this.settings.GetSections(false).Contains("tracker")) {
        this.ttnt.MessageIncomming += this.TTNTrackerMessageIncomming;
        this.tSBatt = this.settings.GetValue("tracker", "batt");
        this.tSLat = this.settings.GetValue("tracker", "lat");
        this.tSLon = this.settings.GetValue("tracker", "lon");
        this.tSHeight = this.settings.GetValue("tracker", "height");
        this.tSSat = this.settings.GetValue("tracker", "sat");
      }
      if (this.settings.GetSections(false).Contains("sensors")) {
        this.ttns.MessageIncomming += this.TTNSensorsMessageIncomming;
      }
    }

    private async void TTNSensorsMessageIncomming(Object _1, BackendEvent e) => await Task.Run(() => {
      Tuple<String, String> json = this.ConvertSensorJson(e.Message);
      if (json != null) {
        this.mqtt.Send(json.Item1, json.Item2);
        Console.WriteLine("Mqtt ->: on " + json.Item1 + " set " + json.Item2);
      }
    });

    private async void TTNTrackerMessageIncomming(Object _1, BackendEvent e) => await Task.Run(() => {
      Tuple<String, String> json = this.ConvertTrackerJson(e.Message);
      if (json != null) {
        this.mqtt.Send(json.Item1, json.Item2);
        Console.WriteLine("Mqtt ->: on "+ json.Item1+" set "+ json.Item2);
      }
    });

    private Tuple<String, String> ConvertSensorJson(String jsonstring) {
      try {
        JsonData json = JsonMapper.ToObject(jsonstring);
        String newjson = JsonMapper.ToJson(new Dictionary<String, Object>() {
          {"Name", json["dev_id"].ToString() },
          {"Rssi", Double.Parse(json["metadata"]["gateways"][0]["rssi"].ToString()) },
          {"Snr", Double.Parse(json["metadata"]["gateways"][0]["snr"].ToString()) },
          {"Temperature", Double.Parse(json["payload_fields"]["temperature"].ToString()) },
          {"Humidity", Double.Parse(json["payload_fields"]["humidity"].ToString()) },
          {"Windspeed", Double.Parse(json["payload_fields"]["windspeed"].ToString()) },
          {"Receivedtime", DateTime.UtcNow } 
        });
        return new Tuple<String, String>("lora/sensor/" + json["dev_id"].ToString(), newjson);
      } catch { }
      return null;
    }

    private Tuple<String, String> ConvertTrackerJson(String jsonstring) {
      try {
        JsonData json = JsonMapper.ToObject(jsonstring);
        Dictionary<String, Object> d = new Dictionary<String, Object>() {
            {"Gps", new Dictionary<String, Object>() },
            {"Rssi", Double.Parse(json["metadata"]["gateways"][0]["rssi"].ToString()) },
            {"Snr", Double.Parse(json["metadata"]["gateways"][0]["snr"].ToString()) },
            {"Receivedtime", DateTime.UtcNow },
            {"Name", json["dev_id"].ToString() }
          };

        if (this.tSBatt != null) {
          d.Add("BatteryLevel", Double.Parse(json["payload_fields"][this.tSBatt].ToString()));
        } else {
          d.Add("BatteryLevel", 0);
        }

        Double lat = 0;
        if (this.tSLat != null) {
          lat = Double.Parse(json["payload_fields"][this.tSLat].ToString());
        }
        ((Dictionary<String, Object>)d["Gps"]).Add("Latitude", lat);

        Double lon = 0;
        if (this.tSLon != null) {
          lon = Double.Parse(json["payload_fields"][this.tSLon].ToString());
        }
        ((Dictionary<String, Object>)d["Gps"]).Add("Longitude", lon);

        ((Dictionary<String, Object>)d["Gps"]).Add("Fix", lat != 0 || lon != 0);

        if (this.tSHeight != null) {
          ((Dictionary<String, Object>)d["Gps"]).Add("Height", Double.Parse(json["payload_fields"][this.tSHeight].ToString()));
        } else {
          ((Dictionary<String, Object>)d["Gps"]).Add("Height", 0);
        }

        if (this.tSSat != null) {
          ((Dictionary<String, Object>)d["Gps"]).Add("Satelites", Double.Parse(json["payload_fields"][this.tSSat].ToString()));
        } 

        String newjson = JsonMapper.ToJson(d);
        return new Tuple<String, String>("lora/data/" + json["dev_id"].ToString(), newjson);
      } catch { }
      return null;
    }

    public override void Dispose() {
      if(this.ttnt != null) {
        this.ttnt.Dispose();
      }
      if(this.ttns != null) {
        this.ttns.Dispose();
      }
      this.mqtt.Dispose();
      base.Dispose();
    }
  }
}