variable "app_id" {
  type = string
}

variable "owner" {
  type = string
}

variable "app_installation_id" {
  type = string
}

variable "pem_file" {
  type = string
}

variable "repos" {
  type = object({
    repositories = list(object({
      name        = string
      description = string
    }))
    repoTeams = list(object({
      teamName = string
    }))
    teamMemberships = list(object({
      teamName = string
      userName = string
    }))
    teamAssignments = list(object({
      teamName = string
      repoName = string
      role     = string
    }))
  })
}



# variable "repos_file" {
#   type = string
# }

locals {
  repos = var.repos #(file(var.repos_file))
}

terraform {
  required_providers {
    github = {
      source  = "integrations/github"
      version = "~> 6.0"
    }
  }
}

# Configure the GitHub Provider
provider "github" {
  owner = var.owner
  app_auth {
    id              = var.app_id              # or `GITHUB_APP_ID`
    installation_id = var.app_installation_id # or `GITHUB_APP_INSTALLATION_ID`
    pem_file        = file(var.pem_file)
  }
}


resource "github_repository" "repositories" {
  for_each = {
    for index, repo in local.repos.repositories :
    repo.name => repo
  }
  name        = each.key
  visibility  = "public"
  auto_init   = true
  description = each.value.description

  template {
    owner                = "nhs-england-tools"
    repository           = "repository-template"
    include_all_branches = false
  }
}

resource "github_repository_ruleset" "default_ruleset" {
  for_each = {
    for index, repo in local.repos.repositories :
    repo.name => repo
  }
  name        = "${each.key}-ruleset"
  repository  = github_repository.repositories[each.key].name
  target      = "branch"
  enforcement = "active"

  conditions {
    ref_name {
      include = ["~ALL"]
      exclude = []
    }
  }

  rules {
    required_signatures = true

  }
}

resource "github_team" "repoTeams" {
 for_each = {
    for index, repoTeam in local.repos.repoTeams :
    repoTeam.teamName => repoTeam
  }
  name        = "${each.key}"
  privacy     = "closed"
}

resource "github_team_membership" "repoTeamMembership" {
 for_each = {
    for index, repoTeamMembership in local.repos.teamMemberships :
    "${repoTeamMembership.teamName}_${repoTeamMembership.userName}"  => repoTeamMembership
  }
  team_id  = "${each.value.teamName}"
  username = "${each.value.userName}"
  role     = "maintainer"
}

resource "github_team_repository" "repoTeamAssignment" {

 for_each = {
    for index, repoTeamAssignment in local.repos.teamAssignments :
    "${repoTeamAssignment.teamName}_${repoTeamAssignment.repoName}"  => repoTeamAssignment
  }

  team_id    = "${each.value.teamName}"
  repository = "${each.value.repoName}"
  permission = "${each.value.role}"
}

output "repositories" {
  value = {
    repositories = resource.github_repository.repositories
    rulesets     = resource.github_repository_ruleset.default_ruleset
    teams        = resource.github_team.repoTeams
    memberships = resource.github_team_membership.repoTeamMembership
    assignments = resource.github_team_repository.repoTeamAssignment
  }
}
