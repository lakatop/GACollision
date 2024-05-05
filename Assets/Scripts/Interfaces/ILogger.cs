public interface ILogger
{
  public void SetAgentId(string id);
  public void SetScenarioId(string id);
  public void SetStartTime(double time);
  public void SetEndTime(double time);
  public void CreateCsv();
  public void AppendCsvLog();
}
