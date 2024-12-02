using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BKGalMgr.Models;
using BKGalMgr.Models.Bangumi;
using BKGalMgr.ViewModels;
using RestSharp;

namespace BKGalMgr.Services;

public class BangumiService
{
    private class HeaderInterceptor(SettingsDto settings) : RestSharp.Interceptors.Interceptor
    {
        public override ValueTask BeforeRequest(RestRequest request, CancellationToken cancellationToken)
        {
            request.AddHeader("Authorization", $"Bearer {settings.Bangumi.AccessToken}");
            return ValueTask.CompletedTask;
        }
    }

    private RestClient _client;

    private readonly SettingsDto _settings;

    public BangumiService(SettingsDto settings)
    {
        _settings = settings;
        // https://bangumi.github.io/api
        _client = new(options =>
        {
            options.BaseUrl = new("https://api.bgm.tv");
            options.UserAgent = "bluelaze/BKGalMgr (https://github.com/bluelaze/BKGalMgr)";
            options.Interceptors = [new HeaderInterceptor(_settings)];
        });
    }

    public async Task<RestResponse<Subject>> GetSubjectAsync(string subjectId)
    {
        var request = new RestRequest($"/v0/subjects/{subjectId}");
        return await _client.ExecuteAsync<Subject>(request);
    }

    public async Task<RestResponse<List<SubjectCharacter>>> GetSubjectCharactersAsync(string subjectId)
    {
        var request = new RestRequest($"/v0/subjects/{subjectId}/characters");
        return await _client.ExecuteAsync<List<SubjectCharacter>>(request);
    }

    public async Task<RestResponse<List<Person>>> GetPersonsAsync(string subjectId)
    {
        var request = new RestRequest($"/v0/subjects/{subjectId}/persons");
        return await _client.ExecuteAsync<List<Person>>(request);
    }

    public async Task<RestResponse<Character>> GetCharactersAsync(string character_id)
    {
        var request = new RestRequest($"/v0/characters/{character_id}");
        return await _client.ExecuteAsync<Character>(request);
    }

    public record GameInfoResponse(GameInfo GameInfo, string ErrorMessage);

    public async Task<string> PullGameInfoAsync(GameInfo gameInfo, string subjectId)
    {
        var subjectResponse = await GetSubjectAsync(subjectId);
        if (!subjectResponse.IsSuccessful)
        {
            return subjectResponse.ErrorException.Message;
        }
        TransformSubjectToGameInfo(gameInfo, subjectResponse.Data);

        var personsResponse = await GetPersonsAsync(subjectId);
        if (!personsResponse.IsSuccessful)
        {
            return personsResponse.ErrorException.Message;
        }
        TransformPersonsToGameInfo(gameInfo, personsResponse.Data);

        var subjectCharactersResponse = await GetSubjectCharactersAsync(subjectId);
        if (!subjectCharactersResponse.IsSuccessful)
        {
            return subjectCharactersResponse.ErrorException.Message;
        }
        TransformSubjectCharactersToGameInfo(gameInfo, subjectCharactersResponse.Data);

        return null;
    }

    public async Task<string> PullGameCharacterInfoAsync(GameInfo gameInfo)
    {
        foreach (var c in gameInfo.Characters)
        {
            if(c.BangumiCharacterId.IsNullOrEmpty())
                continue;

            var charactersResponse = await GetCharactersAsync(c.BangumiCharacterId);
            if (!charactersResponse.IsSuccessful)
            {
                return charactersResponse.ErrorException.Message;
            }
            TransformCharacterToCharacterInfo(gameInfo, c, charactersResponse.Data);
        }
        foreach (var c in gameInfo.Characters)
        {
            await c.SaveIllustration();
        }
        return null;
    }

    public void TransformSubjectToGameInfo(GameInfo gameInfo, Subject subject)
    {
        gameInfo.Name = subject.name;
        if (!subject.name_cn.IsNullOrEmpty())
            gameInfo.Name = $"{subject.name_cn}（{subject.name}）";

        gameInfo.PublishDate = DateTime.ParseExact(subject.date, "yyyy-MM-dd", CultureInfo.CurrentCulture);
        gameInfo.Cover = subject.images.large;
        gameInfo.Story = subject.summary;

        foreach (var item in subject.infobox)
        {
            if (item.key == "website")
                gameInfo.Website = item.value.ToString();
            else if (item.key == "官方网站")
                gameInfo.Website = item.value.ToString();
            else if (item.key == "开发")
                gameInfo.Company = item.value.ToString();
        }

        foreach (var tag in subject.tags)
        {
            gameInfo.Tag.Add(tag.name);
        }
    }

    public void TransformPersonsToGameInfo(GameInfo gameInfo, List<Person> persons)
    {
        foreach (var person in persons)
        {
            if (person.relation == "开发")
                gameInfo.Company = person.name;
            else if (person.relation == "原画")
                gameInfo.Artist.Add(person.name);
            else if (person.relation == "剧本")
                gameInfo.Scenario.Add(person.name);
            else if (person.relation == "音乐")
                gameInfo.Musician.Add(person.name);
            else if (person.relation == "主题歌演出")
                gameInfo.Singer.Add(person.name);
            else if (person.relation == "主题歌作词")
                gameInfo.Singer.Add(person.name);
            else if (person.relation == "主题歌作曲")
                gameInfo.Singer.Add(person.name);
        }
    }

    public void TransformSubjectCharactersToGameInfo(GameInfo gameInfo, List<SubjectCharacter> characters)
    {
        foreach (var character in characters)
        {
            if (character.relation == "主角")
            {
                gameInfo.Characters.Add(
                    new()
                    {
                        Name = character.name,
                        Illustration = character.images.large,
                        CV = character.actors.FirstOrDefault()?.name,
                        BangumiCharacterId = character.id.ToString(),
                        GameFolderPath = gameInfo.FolderPath
                    }
                );
                foreach (var cv in character.actors)
                {
                    gameInfo.Cv.Add(cv.name);
                }
            }
        }
    }

    public void TransformCharacterToCharacterInfo(GameInfo gameInfo, CharacterInfo characterInfo, Character character)
    {
        characterInfo.Description = character.summary;
        characterInfo.Illustration = character.images.large;
        if (character.birth_mon != null && character.birth_day != null)
            characterInfo.Birthday = new(
                gameInfo.PublishDate.Year,
                character.birth_mon ?? 0,
                character.birth_day ?? 0,
                0,
                0,
                0
            );

        foreach (var item in character.infobox)
        {
            if (item.key == "身高")
            {
                // "165cm"
                var valueString = item.value.ToString().ToUpper();
                valueString = valueString.Substring(0, valueString.IndexOf('C'));
                if (double.TryParse(valueString, out double height))
                {
                    characterInfo.Height = height;
                }
            }
            else if (item.key == "BWH")
            {
                // "B89(E)/W65/H88"
                var valueStrings = item.value.ToString().ToUpper().Split('/').Select(i => i.Trim()).ToArray();
                if (valueStrings.Count() != 3)
                    continue;

                var bustString = valueStrings[0];
                var waistString = valueStrings[1];
                var hipsString = valueStrings[2];

                var cupIndex = bustString.IndexOf('(');
                if (cupIndex != -1)
                {
                    var cupIndexEnd = bustString.IndexOf(')');
                    if (cupIndexEnd > cupIndex + 1)
                        characterInfo.Cup = bustString.Substring(cupIndex + 1, cupIndexEnd - cupIndex - 1);
                    else
                        characterInfo.Cup = bustString.Substring(cupIndex + 1);
                    bustString = bustString.Substring(0, cupIndex);
                }
                if (bustString.StartsWith('B'))
                    bustString = bustString.Substring(1);
                if (double.TryParse(bustString, out double bust))
                    characterInfo.Bust = bust;

                if (waistString.StartsWith('W'))
                    waistString = waistString.Substring(1);
                if (double.TryParse(waistString, out double waist))
                    characterInfo.Waist = waist;

                if (hipsString.StartsWith('H'))
                    hipsString = hipsString.Substring(1);
                if (double.TryParse(hipsString, out double hips))
                    characterInfo.Hips = hips;
            }
        }
    }
}
