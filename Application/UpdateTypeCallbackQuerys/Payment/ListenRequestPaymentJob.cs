using System.Net;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Quartz;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using yoomoney_api.notification;

namespace Application.UpdateTypeCallbackQuerys.Payment;

public class ListenRequestPaymentJob : IJob
{
    public string JobId { get; set; } 
    public  IServiceProvider ServiceProvider { get; set; }
    
    public async Task Execute(IJobExecutionContext context)
    {
       
    }
}