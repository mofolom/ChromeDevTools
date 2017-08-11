﻿using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
using System;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace MasterDevs.ChromeDevTools.Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                // STEP 1 - Run Chrome
                var chromeProcessFactory = new ChromeProcessFactory(new StubbornDirectoryCleaner());
                using (var chromeProcess = chromeProcessFactory.Create(9222))
                {
                    // STEP 2 - Create a debugging session
                    var sessionInfo = (await chromeProcess.GetSessionInfo()).LastOrDefault();
                    var chromeSessionFactory = new ChromeSessionFactory();
                    var chromeSession = chromeSessionFactory.Create(sessionInfo.WebSocketDebuggerUrl);

                    // STEP 3 - Send a command
                    //
                    // Here we are sending a command to tell chrome to navigate to
                    // the specified URL
                    var navigateResponse = chromeSession.SendAsync(new NavigateCommand
                    {
                        Url = "http://www.google.com"
                    })
                        .Result;
                    Console.WriteLine("NavigateResponse: " + navigateResponse.Id);

                    // STEP 4 - Register for events (in this case, "Page" domain events)
                    // send an event to tell chrome to send us all Page events
                    // but we only subscribe to certain events in this session
                    var pageEnableResult = chromeSession.SendAsync<ChromeDevTools.Protocol.Chrome.Page.EnableCommand>().Result;
                    Console.WriteLine("PageEnable: " + pageEnableResult.Id);
                    chromeSession.Subscribe<Protocol.Chrome.Page.DomContentEventFiredEvent>(domContentEvent =>
                    {
                        Console.WriteLine("DomContentEvent: " + domContentEvent.Timestamp);
                    });
                    // you might never see this, but that's what an event is ... right?
                    chromeSession.Subscribe<Protocol.Chrome.Page.FrameStartedLoadingEvent>(frameStartedLoadingEvent =>
                    {
                        Console.WriteLine("FrameStartedLoading: " + frameStartedLoadingEvent.FrameId);
                    });

                    Console.ReadLine();
                }
            }).Wait();
        }
    }
}