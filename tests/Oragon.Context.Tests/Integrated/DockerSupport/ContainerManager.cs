﻿using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Linq;

namespace Oragon.Context.Tests.Integrated.DockerSupport
{
    public class ContainerManager : IDisposable, IProgress<JSONMessage>
    {
        public ContainerManager(DockerClient docker)
        {
            this.Docker = docker;

        }

        private ContainerManager(DockerClient docker, string id) : this(docker)
        {
            this.CreateResponse = new CreateContainerResponse() { ID = id };
        }

        public DockerClient Docker { get; }


        public CreateContainerResponse CreateResponse { get; private set; }

        public void Create(CreateContainerParameters createParameters)
        {
            this.Docker.Images.CreateImageAsync(new ImagesCreateParameters() { FromImage = createParameters.Image }, null, this).GetAwaiter().GetResult();
            this.CreateResponse = this.Docker.Containers.CreateContainerAsync(createParameters).GetAwaiter().GetResult();
            if (this.CreateResponse.Warnings != null && this.CreateResponse.Warnings.Any())
            {
                throw new InvalidOperationException(string.Join(" | ", this.CreateResponse.Warnings));
            }
        }

        public bool Start(ContainerStartParameters startRequest)
        {
            return this.Docker.Containers.StartContainerAsync(this.CreateResponse.ID, startRequest).GetAwaiter().GetResult();
        }

        public ContainerInspectResponse Inspect()
        {
            ContainerInspectResponse containerInspectResponse = this.Docker.Containers.InspectContainerAsync(this.CreateResponse.ID).GetAwaiter().GetResult();
            return containerInspectResponse;
        }

        public void WaitUntilTextFoundInLog(ContainerLogsParameters containerLogsParameters, string textToFind, int getLogsRetryCount, TimeSpan getLogsWaitTime)
        {
            string logs = null;
            bool isOk = false;
            for (int i = 0; i < getLogsRetryCount; i++)
            {
                System.Threading.Thread.Sleep(getLogsWaitTime);

                using (System.IO.Stream logStream = this.Docker.Containers.GetContainerLogsAsync(this.CreateResponse.ID, containerLogsParameters).GetAwaiter().GetResult())
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(logStream))
                    {
                        logs = reader.ReadToEnd();
                    }
                    if (!string.IsNullOrWhiteSpace(logs))
                    {
                        isOk = logs.Contains(textToFind);
                        if (isOk)
                        {
                            break;
                        }
                    }
                }
            }
            if (!isOk)
            {
                throw new TimeoutException("Timeout waiting logs");
            }
        }


        public void Dispose()
        {
            if (this.CreateResponse != null)
            {
                this.Docker.Containers.StopContainerAsync(this.CreateResponse.ID, new ContainerStopParameters() { WaitBeforeKillSeconds = 30 }).GetAwaiter().GetResult();
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(30));
                this.Docker.Containers.RemoveContainerAsync(this.CreateResponse.ID, new ContainerRemoveParameters() { Force = true, RemoveVolumes = true }).GetAwaiter().GetResult();
            }
        }

        public void Report(JSONMessage value)
        {
            System.Diagnostics.Debug.WriteLine($"{value.Status} | {value.ProgressMessage}");
        }

        internal static ContainerManager GetCurrent(DockerClient docker)
        {
            string machineName = System.Environment.MachineName;
            System.Collections.Generic.IList<ContainerListResponse> containers = docker.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).GetAwaiter().GetResult();

            ContainerListResponse container = containers.SingleOrDefault(it => it.ID.StartsWith(machineName) || it.ID.EndsWith(machineName));

            if (container != null)
            {
                return new ContainerManager(docker, container.ID);
            }

            return null;

        }
    }
}