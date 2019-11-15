using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlubbFish.Utils;
using BlubbFish.Utils.IoT.Bots;
using BlubbFish.Utils.IoT.Connector;
using LitJson;

namespace Fraunhofer.Fit.IoT.TTN.Bridge {
  class Program : ABot {
    private ADataBackend ttn;
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

    private void Attach() {
      this.ttn.MessageIncomming += this.Ttn_MessageIncomming;
    }

    private void Connect(InIReader settings) {
      this.ttn = (ADataBackend)ABackend.GetInstance(settings.GetSection("from"), ABackend.BackendType.Data);
      this.mqtt = (ADataBackend)ABackend.GetInstance(settings.GetSection("to"), ABackend.BackendType.Data);
    }

    private async void Ttn_MessageIncomming(Object sender, BlubbFish.Utils.IoT.Events.BackendEvent e) => await Task.Run(() => {
      Tuple<String, String> json = this.ConvertJson(e.Message);
      if (json != null) {
        this.mqtt.Send("lora/data/" + json.Item1, json.Item2);
        Console.WriteLine("Koordinate konvertiert.");
      }
    });

    private Tuple<String, String> ConvertJson(String jsonstring) {
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
      this.ttn.Dispose();
      this.mqtt.Dispose();
      base.Dispose();
    }
  }
}