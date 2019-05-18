﻿using System.Threading.Tasks;
using Edi.Practice.RequestResponseModel;

namespace Moonglade.Configuration.Abstraction
{
    public interface IBlogConfig
    {
        BlogOwnerSettings BlogOwnerSettings { get; set; }
        GeneralSettings GeneralSettings { get; set; }
        ContentSettings ContentSettings { get; set; }
        EmailConfiguration EmailConfiguration { get; set; }
        FeedSettings FeedSettings { get; set; }
        WatermarkSettings WatermarkSettings { get; set; }

        Task<Response> SaveConfigurationAsync<T>(T moongladeSettings) where T : MoongladeSettings;

        string EncryptPassword(string clearPassword);

        void RequireRefresh();
    }
}