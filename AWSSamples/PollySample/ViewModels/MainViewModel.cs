using System;
using System.IO;
using System.Net;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using PollySample.Helpers;

namespace PollySample.ViewModels
{
    public class MainViewModel : Observable
    {
        private string _inputText;
        private IRandomAccessStream _randomAccessStream;

        /// <summary>
        ///     생성자
        /// </summary>
        public MainViewModel()
        {
            Init();
        }

        /// <summary>
        ///     입력택스트
        /// </summary>
        public string InputText
        {
            get => _inputText;
            set => Set(ref _inputText, value);
        }

        /// <summary>
        ///     TTS커맨드
        /// </summary>
        public ICommand TTSCommand { get; set; }

        /// <summary>
        ///     랜덤 엑세스 스트림
        /// </summary>
        public IRandomAccessStream RandomAccessStream
        {
            get => _randomAccessStream;
            set => Set(ref _randomAccessStream, value);
        }

        /// <summary>
        ///     미디어 종료 커맨드
        /// </summary>
        public ICommand MediaEndedCommand { get; set; }

        /// <summary>
        ///     초기화
        /// </summary>
        private void Init()
        {
            TTSCommand = new RelayCommand(ExecuteTTSCommand);
            MediaEndedCommand = new RelayCommand(ExecuteMediaEndedCommand);
        }

        /// <summary>
        ///     미디어 종료 커맨드 실행
        /// </summary>
        private void ExecuteMediaEndedCommand()
        {
        }

        /// <summary>
        ///     TTS 커맨드 실행
        /// </summary>
        private async void ExecuteTTSCommand()
        {
            if (string.IsNullOrEmpty(InputText)) return;

            //폴리 클라이언트 생성
            var pc = new AmazonPollyClient("AccessKeyID를 입력하세요", "SecretAccessKey를 입력하세요"
                , RegionEndpoint.APNortheast2);

            //요청 생성
            var sreq = new SynthesizeSpeechRequest
            {
                Text = $"<speak>{InputText}</speak>",
                OutputFormat = OutputFormat.Mp3,
                VoiceId = VoiceId.Seoyeon,
                LanguageCode = "ko-KR",
                TextType = TextType.Ssml
            };

            InputText = string.Empty;

            //서비스 요청
            var sres = await pc.SynthesizeSpeechAsync(sreq);

            //서비스 요청 결과 확인
            if (sres.HttpStatusCode != HttpStatusCode.OK)
                return;

            //파일명 생성
            var fileName = $@"{ApplicationData.Current.LocalFolder.Path}\{DateTime.Now:yyMMddhhmmss}.mp3";
            //파일에 AudioStream 쓰기
            using (var fileStream = File.Create(fileName))
            {
                sres.AudioStream.CopyTo(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
            //생성된 파일을 가져오기
            var file = await StorageFile.GetFileFromPathAsync(fileName);
            //파일을 열어서 RandomAccessStream 프로퍼티에 입력
            RandomAccessStream = await file.OpenAsync(FileAccessMode.Read);

            //RandomAccessStream과 바인딩이 되어있는 MediaBehavior에서 MediaPlayer를 통해서 재생
        }
    }
}
