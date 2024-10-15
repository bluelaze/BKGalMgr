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

    public async Task<RestResponse<List<Character>>> GetCharactersAsync(string subjectId)
    {
        var request = new RestRequest($"/v0/subjects/{subjectId}/characters");
        return await _client.ExecuteAsync<List<Character>>(request);
    }

    public async Task<RestResponse<List<Person>>> GetPersonsAsync(string subjectId)
    {
        var request = new RestRequest($"/v0/subjects/{subjectId}/persons");
        return await _client.ExecuteAsync<List<Person>>(request);
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

        var CharactersResponse = await GetCharactersAsync(subjectId);
        if (!CharactersResponse.IsSuccessful)
        {
            return CharactersResponse.ErrorException.Message;
        }
        TransformCharactersToGameInfo(gameInfo, CharactersResponse.Data);

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

    public void TransformCharactersToGameInfo(GameInfo gameInfo, List<Character> characters)
    {
        foreach (var character in characters)
        {
            if (character.relation == "主角")
            {
                gameInfo.Characters.Add(new() { Name = character.name, BangumiCharacterId = character.id.ToString(), });
                foreach (var cv in character.actors)
                {
                    gameInfo.Cv.Add(cv.name);
                }
            }
        }
    }
}
