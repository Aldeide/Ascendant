using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    public struct SavedStructureData
    {
        public string StructureId;
        public string SystemId;
        public string Type;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Health;
        public ResourceInventoryState Inventory;
    }

    public class WorldDatabaseRepository
    {
        private readonly SqliteConnection m_Connection;

        public WorldDatabaseRepository(SqliteConnection connection)
        {
            m_Connection = connection;
        }

        public void CreateTables()
        {
            using (var cmd = m_Connection.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS star_systems (
                        system_id TEXT PRIMARY KEY,
                        controlling_faction TEXT
                    );

                    CREATE TABLE IF NOT EXISTS structures (
                        structure_id TEXT PRIMARY KEY,
                        system_id TEXT,
                        type TEXT,
                        pos_x REAL,
                        pos_y REAL,
                        pos_z REAL,
                        rot_x REAL,
                        rot_y REAL,
                        rot_z REAL,
                        rot_w REAL,
                        health REAL,
                        ore INTEGER,
                        gas INTEGER,
                        fuel INTEGER,
                        munitions INTEGER,
                        components INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS player_profiles (
                        player_guid TEXT PRIMARY KEY,
                        rank TEXT,
                        wcp INTEGER,
                        tech_progress TEXT
                    );
                ";
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveStarSystem(string systemId, string faction)
        {
            using (var cmd = m_Connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO star_systems (system_id, controlling_faction)
                    VALUES (@systemId, @faction);
                ";
                cmd.Parameters.AddWithValue("@systemId", systemId);
                cmd.Parameters.AddWithValue("@faction", faction);
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveStructure(string structureId, string systemId, string type, Vector3 position, Quaternion rotation, float health, ResourceInventoryState inventory)
        {
            using (var cmd = m_Connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO structures (
                        structure_id, system_id, type, 
                        pos_x, pos_y, pos_z, 
                        rot_x, rot_y, rot_z, rot_w, 
                        health, ore, gas, fuel, munitions, components
                    ) VALUES (
                        @structureId, @systemId, @type, 
                        @posX, @posY, @posZ, 
                        @rotX, @rotY, @rotZ, @rotW, 
                        @health, @ore, @gas, @fuel, @munitions, @components
                    );
                ";
                cmd.Parameters.AddWithValue("@structureId", structureId);
                cmd.Parameters.AddWithValue("@systemId", systemId);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@posX", position.x);
                cmd.Parameters.AddWithValue("@posY", position.y);
                cmd.Parameters.AddWithValue("@posZ", position.z);
                cmd.Parameters.AddWithValue("@rotX", rotation.x);
                cmd.Parameters.AddWithValue("@rotY", rotation.y);
                cmd.Parameters.AddWithValue("@rotZ", rotation.z);
                cmd.Parameters.AddWithValue("@rotW", rotation.w);
                cmd.Parameters.AddWithValue("@health", health);
                cmd.Parameters.AddWithValue("@ore", inventory.Ore);
                cmd.Parameters.AddWithValue("@gas", inventory.Gas);
                cmd.Parameters.AddWithValue("@fuel", inventory.Fuel);
                cmd.Parameters.AddWithValue("@munitions", inventory.Munitions);
                cmd.Parameters.AddWithValue("@components", inventory.Components);
                cmd.ExecuteNonQuery();
            }
        }

        public List<SavedStructureData> LoadStructures(string systemId)
        {
            var list = new List<SavedStructureData>();
            using (var cmd = m_Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM structures WHERE system_id = @systemId;";
                cmd.Parameters.AddWithValue("@systemId", systemId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = new SavedStructureData
                        {
                            StructureId = reader.GetString(reader.GetOrdinal("structure_id")),
                            SystemId = reader.GetString(reader.GetOrdinal("system_id")),
                            Type = reader.GetString(reader.GetOrdinal("type")),
                            Position = new Vector3(
                                Convert.ToSingle(reader["pos_x"]),
                                Convert.ToSingle(reader["pos_y"]),
                                Convert.ToSingle(reader["pos_z"])
                            ),
                            Rotation = new Quaternion(
                                Convert.ToSingle(reader["rot_x"]),
                                Convert.ToSingle(reader["rot_y"]),
                                Convert.ToSingle(reader["rot_z"]),
                                Convert.ToSingle(reader["rot_w"])
                            ),
                            Health = Convert.ToSingle(reader["health"]),
                            Inventory = new ResourceInventoryState
                            {
                                Ore = Convert.ToInt32(reader["ore"]),
                                Gas = Convert.ToInt32(reader["gas"]),
                                Fuel = Convert.ToInt32(reader["fuel"]),
                                Munitions = Convert.ToInt32(reader["munitions"]),
                                Components = Convert.ToInt32(reader["components"])
                            }
                        };
                        list.Add(data);
                    }
                }
            }
            return list;
        }

        public void DeleteStructure(string structureId)
        {
            using (var cmd = m_Connection.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM structures WHERE structure_id = @structureId;";
                cmd.Parameters.AddWithValue("@structureId", structureId);
                cmd.ExecuteNonQuery();
            }
        }

        public void SavePlayerProfile(string guid, string rank, int wcp, string techProgress)
        {
            using (var cmd = m_Connection.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO player_profiles (player_guid, rank, wcp, tech_progress)
                    VALUES (@guid, @rank, @wcp, @techProgress);
                ";
                cmd.Parameters.AddWithValue("@guid", guid);
                cmd.Parameters.AddWithValue("@rank", rank);
                cmd.Parameters.AddWithValue("@wcp", wcp);
                cmd.Parameters.AddWithValue("@techProgress", techProgress);
                cmd.ExecuteNonQuery();
            }
        }

        public bool LoadPlayerProfile(string guid, out string rank, out int wcp, out string techProgress)
        {
            rank = "Ensign";
            wcp = 0;
            techProgress = "";

            using (var cmd = m_Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM player_profiles WHERE player_guid = @guid;";
                cmd.Parameters.AddWithValue("@guid", guid);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rank = reader.GetString(reader.GetOrdinal("rank"));
                        wcp = reader.GetInt32(reader.GetOrdinal("wcp"));
                        techProgress = reader.GetString(reader.GetOrdinal("tech_progress"));
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
