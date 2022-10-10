using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OptimaJet.Workflow.Core;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Runtime;

namespace WorkflowLib;

public sealed class WeatherActivity : ActivityBase
{
    public WeatherActivity()
    {
        Type = "WeatherActivity";
        Title = "Weather forecast";
        Description = "Get weather forecast via API";

        // the file name with your form template, without extension
        Template = "weatherActivity";
        // the file name with your svg template, without extension
        SVGTemplate = "weatherActivity";
    }

    public override async Task ExecutionAsync(WorkflowRuntime runtime, ProcessInstance processInstance,
        Dictionary<string, string> parameters, CancellationToken token)
    {
        const string url = "https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&daily=temperature_2m_min&timezone=GMT";
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url, token);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(token);
            using var streamReader = new StreamReader(stream);
            var data = await streamReader.ReadToEndAsync();
            var json = JsonConvert.DeserializeObject<JObject>(data);
            var daily = json?["daily"];
            var date = daily?["time"]?[0] ?? "Date is not defined";
            var temperature = daily?["temperature_2m_min"]?[0] ?? "Temperature is not defined";

            // store the entire response to the Weather process parameter
            await processInstance.SetParameterAsync("Weather", json, ParameterPurpose.Persistence);
            // store the weather date in the WeatherDate process parameter
            await processInstance.SetParameterAsync("WeatherDate", date, ParameterPurpose.Persistence);
            // store the temperature in the process parameter WeatherTemperature
            await processInstance.SetParameterAsync("WeatherTemperature", temperature, ParameterPurpose.Persistence);
        }
    }

    public override async Task PreExecutionAsync(WorkflowRuntime runtime, ProcessInstance processInstance,
        Dictionary<string, string> parameters, CancellationToken token)
    {
        // do nothing
    }
}