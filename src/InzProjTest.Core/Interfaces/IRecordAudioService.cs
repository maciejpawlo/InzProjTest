namespace InzProjTest.Core.Interfaces
{
    public interface IRecordAudioService
    {
        void StartRecording(string filepath);
        void StopRecording();
    }
}
