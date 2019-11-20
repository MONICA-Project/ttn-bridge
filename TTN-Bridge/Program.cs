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

    static void Main(String[] args) => new Program();

    Program() {
      InIReader.SetSearchPath(new List<String>() { "/etc/ttnbridge", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ttnbridge" });
      if (!InIReader.ConfigExist("settings")) {
        Helper.WriteError("No settings.ini found. Abord!");
        return;
      }
      InIReader settings = InIReader.GetInstance("settings");
      this.logger.SetPath(settings.GetValue("logging", "path"));

      this.Connect(settings);
      this.Attach();
      this.WaitForShutdown();
      this.Dispose();
    }

    private void Connect(InIReader settings) {
      this.ttnt = (ADataBackend)ABackend.GetInstance(settings.GetSection("tracker"), ABackend.BackendType.Data);
      this.ttns = (ADataBackend)ABackend.GetInstance(settings.GetSection("sensors"), ABackend.BackendType.Data);
      this.mqtt = (ADataBackend)ABackend.GetInstance(settings.GetSection("to"), ABackend.BackendType.Data);
    }

    private void Attach() {
      this.ttnt.MessageIncomming += this.TTNTrackerMessageIncomming;
      this.ttns.MessageIncomming += this.TTNSensorsMessageIncomming;
    }

    private async void TTNSensorsMessageIncomming(Object sender, BackendEvent e) => await Task.Run(() => {
      Tuple<String, String> json = this.ConvertSensorJson(e.Message);
      if (json != null) {
        this.mqtt.Send("lora/sensor/" + json.Item1, json.Item2);
        Console.WriteLine("Umweltdaten konvertiert.");
      }
    });

    private async void TTNTrackerMessageIncomming(Object sender, BackendEvent e) => await Task.Run(() => {
      Tuple<String, String> json = this.ConvertTrackerJson(e.Message);
      if (json != null) {
        this.mqtt.Send("lora/data/" + json.Item1, json.Item2);
        Console.WriteLine("Koordinate konvertiert.");
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
        return new Tuple<String, String>(json["dev_id"].ToString(), newjson);
      } catch { }
      return null;
    }

    private Tuple<String, String> ConvertTrackerJson(String jsonstring) {
      try {
        JsonData json = JsonMapper.ToObject(jsonstring);
        String newjson = JsonMapper.ToJson(new Dictionary<String, Object>() {
            {"Gps", new Dictionary<String, Object>() {
              {"Fix", true },
              {"Hdop", 0.9 },
              {"Height", Double.Parse(json["payload_fields"]["alt"].ToString()) },
              {"Latitude", Double.Parse(json["payload_fields"]["lat"].ToString()) },
              {"Longitude", Double.Parse(json["payload_fields"]["lon"].ToString()) },
              {"LastLatitude", Double.Parse(json["payload_fields"]["lat"].ToString()) },
              {"LastLongitude", Double.Parse(json["payload_fields"]["lon"].ToString()) },
              {"LastGPSPos", DateTime.UtcNow }
            } },
            {"Name", json["dev_id"].ToString() },
            {"Rssi", Double.Parse(json["metadata"]["gateways"][0]["rssi"].ToString()) },
            {"Snr", Double.Parse(json["metadata"]["gateways"][0]["snr"].ToString()) },
            {"Receivedtime", DateTime.UtcNow },
            {"BatteryLevel", 4.0 }
          });
        return new Tuple<String, String>(json["dev_id"].ToString(), newjson);
      } catch { }
      return null;
    }

    public override void Dispose() {
      this.ttnt.Dispose();
      this.ttns.Dispose();
      this.mqtt.Dispose();
      base.Dispose();
    }
  }
}