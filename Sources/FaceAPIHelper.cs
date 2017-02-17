using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace FaceXamarin
{
    public class FaceAPIHelper
    {
        private static FaceAPIHelper _instance;

        // Create an empty person group
        private readonly string _personGroupId = "myfriends";

        private readonly IFaceServiceClient faceServiceClient =
            new FaceServiceClient("61c486a29e6e4fd7918ec99b8740b8f7");

        private bool _init;

        /// <summary>
        ///     생성자
        /// </summary>
        public FaceAPIHelper()
        {
            Init();
        }

        /// <summary>
        ///     인스턴스
        /// </summary>
        public static FaceAPIHelper Instance
        {
            get { return _instance = _instance ?? new FaceAPIHelper(); }
        }

        /// <summary>
        ///     초기화
        /// </summary>
        public async void Init()
        {
            if (_init) return;

            //사람그룹 목록을 조회한 후에
            var list = await faceServiceClient.ListPersonGroupsAsync();
            //myFriends란 아이디를 가지고 있지 않다면, 사람그룹을 생성한다.
            if (list.All(p => p.PersonGroupId != _personGroupId))
                await faceServiceClient.CreatePersonGroupAsync(_personGroupId, "My Friends");

            _init = true;
        }

        /// <summary>
        ///     사진에서 얼굴 디텍트
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public async Task<Face[]> GetDetectAsync(Stream fileStream)
        {
            return await faceServiceClient.DetectAsync(fileStream);
        }

        /// <summary>
        ///     사람 추가
        /// </summary>
        /// <param name="personName"></param>
        /// <returns></returns>
        public async Task<Guid> CreatePersonAsync(string personName)
        {
            var persons = await faceServiceClient.GetPersonsAsync(_personGroupId);
            if (persons.Any(p => p.Name == personName))
            {
                var per = persons.First(p => p.Name == personName);
                return per.PersonId;
            }
            var result = await faceServiceClient.CreatePersonAsync(_personGroupId, personName);
            return result.PersonId;
        }

        /// <summary>
        ///     사람 삭제
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task DeletePersonAsync(Guid personId)
        {
            await faceServiceClient.DeletePersonAsync(_personGroupId, personId);
        }

        /// <summary>
        ///     등록된 사람들 정보 조회
        /// </summary>
        /// <returns></returns>
        public async Task<Person[]> GetPersonsAsync()
        {
            var persons = await faceServiceClient.GetPersonsAsync(_personGroupId);
            return persons;
        }

        /// <summary>
        ///     사람에 얼굴 등록하기
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public async Task<AddPersistedFaceResult> AddPersonFaceAsync(Guid personId, Stream fileStream)
        {
            try
            {
                // Detect faces in the image and add to Anna
                var result = await faceServiceClient.AddPersonFaceAsync(
                    _personGroupId, personId, fileStream);
                return result;
            }
            catch (FaceAPIException fae)
            {
                Debug.WriteLine(fae.ErrorMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        ///     트레이닝 사람그룹
        /// </summary>
        /// <returns></returns>
        public async Task TrainPersonGroupAsync()
        {
            await faceServiceClient.TrainPersonGroupAsync(_personGroupId);

            while (true)
            {
                var trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(_personGroupId);
                if (trainingStatus.Status != Status.Running)
                    break;
                await Task.Delay(1000);
            }
        }

        /// <summary>
        ///     인증하기
        /// </summary>
        /// <param name="faceIds"></param>
        /// <returns></returns>
        public async Task<IdentifyResult[]> IdentifyAsync(Guid[] faceIds)
        {
            try
            {
                var result = await faceServiceClient.IdentifyAsync(_personGroupId, faceIds);
                return result;
            }
            catch (FaceAPIException fae)
            {
                Debug.WriteLine(fae.ErrorMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        ///     사람 정보 조회
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<Person> GetPersonAsync(Guid personId)
        {
            var result = await faceServiceClient.GetPersonAsync(_personGroupId, personId);
            return result;
        }
    }
}