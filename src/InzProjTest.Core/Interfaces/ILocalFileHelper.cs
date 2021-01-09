namespace InzProjTest.Core.Interfaces
{
    public interface ILocalFileHelper
    {
        string GetPath(string filename);
        string GetPatientPath(string filename, string firstName, string lastName);
    }
}
