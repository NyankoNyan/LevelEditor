using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.API
{
    /// <summary>
    /// Роль пользователя. Включает описание всего, что пользователь может делать.
    /// </summary>
    [Serializable]
    public struct Role
    {
        public string name;
        public bool isDefault;
        public bool allLayersAvailable;
        public bool allGridsAvailable;
        public string[] availableLayers;
        public string[] availableGrids;

        public static Role operator +(Role v1, Role v2)
        {
            Role result = new();
            result.allLayersAvailable = v1.allLayersAvailable || v2.allLayersAvailable;
            result.allGridsAvailable = v1.allGridsAvailable || v2.allGridsAvailable;

            if(v1.availableLayers!=null)
                result.availableLayers = v1.availableLayers.Union(v2.availableLayers).Distinct().ToArray();
            else
                result.availableLayers = v2.availableLayers;
            if(v1.availableGrids!=null)
                result.availableGrids = v1.availableGrids.Union(v2.availableGrids).Distinct().ToArray();
            else
                result.availableGrids = v2.availableGrids;

            return result;
        }
    }

    /// <summary>
    /// Полное описание состояния пользователя. Личный сейв файл.
    /// </summary>
    public class UserState
    {
        public string identities;
        public bool isAdmin;
        public bool onServer;
        public ulong clientId;
        public List<string> roles = new();
        public int avatarId;
    }

    /// <summary>
    /// Хранит в себе права и настройки всех пользователей, когда либо зашедших на сервер.
    /// Для одиночного режима хранит настройки единственного игрока.
    /// </summary>
    public class UserManager
    {
        public Action<Role> roleAdded;
        public Action<string> roleRemoved;
        public Action<Role> roleChanged;
        private List<Role> _roles = new();
        private List<UserState> _userStates = new();

        public void CheckNewRole(Role role)
        {
            if (_roles.Any(x => x.name == role.name)) {
                throw new LevelAPIException($"Role {role.name} already exists");
            }
            CheckRole(role);
        }

        public void CheckRole(Role role) { }

        public void AddRole(Role role)
        {
            CheckNewRole(role);
            _roles.Add(role);
            roleAdded?.Invoke(role);
        }

        public void RemoveRole(string name, bool removeInUsers)
        {
            int index = _roles.FindIndex(x => x.name == name);
            if (index >= 0) {
                _roles.RemoveAt(index);
            } else {
                throw new LevelAPIException($"Role {name} not exists");
            }
            if (removeInUsers) {
                foreach (var userState in _userStates) {
                    while (userState.roles.Remove(name)) { }
                }
            }
            roleRemoved?.Invoke(name);
        }

        public IEnumerable<string> GetRoleNames()
        {
            return _roles.Select(x => x.name);
        }

        public Role GetRole(string name)
        {
            try {
                return _roles.Single(x => x.name == name);
            } catch {
                throw new LevelAPIException($"Role {name} not found");
            }
        }

        public void ChangeRole(Role role)
        {
            int index = _roles.FindIndex(x => x.name == role.name);
            if (index == -1) {
                throw new LevelAPIException($"Role {role.name} not exists");
            }
            CheckRole(role);

            _roles[index] = role;
            roleChanged?.Invoke(role);
        }

        public bool HasUser(string identities)
        {
            return _userStates.Any(x => x.identities == identities);
        }

        public UserState GetUserState(string indentities)
        {
            try {
                return _userStates.Single(x => x.identities == indentities);
            } catch {
                throw new LevelAPIException($"User {indentities} not exists");
            }
        }

        public UserState CreateNewUser(string identities)
        {
            UserState userState = new();
            userState.identities = identities;
            foreach (var role in _roles.Where(x => x.isDefault)) {
                userState.roles.Add(role.name);
            }
            return userState;
        }

        public Role SummarizeUserRoles(string identities)
        {
            UserState userState = GetUserState(identities);
            Role summary = new();
            foreach (var roleName in userState.roles) {
                Role role;
                try {
                    role = GetRole(roleName);
                } catch (LevelAPIException e) {
                    Debug.LogError(e.Message);
                    continue;
                }
                summary += role;
            }
            return summary;
        }
    }
}