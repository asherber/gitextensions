﻿using System;
using System.Collections.Generic;
using System.Linq;
using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    public class GithubRepo : IHostedRepository
    {
        private Repository _repo;

        public GithubRepo(Repository repo)
        {
            _repo = repo;
        }

        public string Owner => _repo.Owner?.Login;
        public string Name => _repo.Name;
        public string Description => _repo.Description;
        public bool IsAFork => _repo.Fork;
        public bool IsMine => Owner == GithubLoginInfo.Username;
        public bool IsPrivate => _repo.Private;
        public int Forks => _repo.Forks;
        public string Homepage => _repo.Homepage;

        public string ParentReadOnlyUrl
        {
            get
            {
                if (!_repo.Fork)
                {
                    return null;
                }

                if (!_repo.Detailed)
                {
                    if (_repo.Organization != null)
                    {
                        return null;
                    }

                    _repo = Github3Plugin.github.getRepository(Owner, Name);
                }

                return _repo.Parent?.GitUrl;
            }
        }

        public string ParentOwner
        {
            get
            {
                if (!_repo.Fork)
                {
                    return null;
                }

                if (!_repo.Detailed)
                {
                    if (_repo.Organization != null)
                    {
                        return null;
                    }

                    _repo = Github3Plugin.github.getRepository(Owner, Name);
                }

                return _repo.Parent?.Owner.Login;
            }
        }

        public string CloneReadWriteUrl => _repo.SshUrl;
        public string CloneReadOnlyUrl => _repo.GitUrl;

        public List<IHostedBranch> Branches
            => _repo.GetBranches().Select(branch => new GithubBranch(branch)).ToList<IHostedBranch>();

        public IHostedRepository Fork()
        {
            return new GithubRepo(_repo.CreateFork());
        }

        public IReadOnlyList<IPullRequestInformation> GetPullRequests()
        {
            var pullRequests = _repo?.GetPullRequests();

            if (pullRequests != null)
            {
                return pullRequests
                    .Select(pr => new GithubPullRequest(pr))
                    .ToList();
            }

            return Array.Empty<IPullRequestInformation>();
        }

        public int CreatePullRequest(string myBranch, string remoteBranch, string title, string body)
        {
            var pullRequest = _repo.CreatePullRequest(GithubLoginInfo.Username + ":" + myBranch, remoteBranch, title, body);

            if (pullRequest == null || pullRequest.Number == 0)
            {
                throw new Exception("Failed to create pull request.");
            }

            return pullRequest.Number;
        }

        public override string ToString() => $"{Owner}/{Name}";
    }
}
