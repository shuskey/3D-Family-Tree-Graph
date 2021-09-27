﻿using Assets.Scripts.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Assets.Scripts.DataProviders
{
    class ListOfPersonsFromDataBase
    {
        public List<Person> personsList;
        private string _dataBaseFileName;

        public ListOfPersonsFromDataBase(string DataBaseFileName)           
        {
            _dataBaseFileName = DataBaseFileName;
            personsList = new List<Person>();
        }
        public void GetSinglePersonFromDataBase(int ownerId, int generation, float xOffset, int spouseNumber)
        {
            // only if this person is not in the Tribe yet
            if (!personsList.Exists(x => x.dataBaseOwnerId == ownerId))
                GetListOfPersonsFromDataBase(limitListSizeTo: 1, ownerId, generation, xOffset, spouseNumber);
        }

        public void GetListOfPersonsFromDataBase(int limitListSizeTo, int? JustThisOwnerId = null, int generation = 0, 
            float xOffset = 0.0f, int spouseNumber = 0)
        {
            string conn = "URI=file:" + Application.dataPath + $"/RootsMagic/{_dataBaseFileName}";
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            string QUERYNAMES =
                "SELECT  name.OwnerID \n" +
                "     , case when Sex = 0 then 'M' when Sex = 1 then 'F' else 'U' end \n" +
                "     , name.Given, name.Surname \n" +
                "     , CASE WHEN SUBSTR(eventBirth.Date,8,2) THEN SUBSTR(eventBirth.Date,8,2) ELSE \"0\" END AS BirthMonth \n"+
                "     , CASE WHEN SUBSTR(eventBirth.Date, 10, 2) THEN SUBSTR(eventBirth.Date,10,2) ELSE \"0\" END AS BirthdDay \n" +
                "     , CASE WHEN SUBSTR(eventBirth.Date,4,4) THEN \n" +
                "           CASE WHEN SUBSTR(eventBirth.Date, 4, 4) != \"0\" THEN SUBSTR(eventBirth.Date,4,4) END \n" +
                "           ELSE CAST(name.BirthYear as varchar(10)) END AS BirthYear \n" +
                "     , person.Living \n" +
                "     , CASE WHEN SUBSTR(eventDeath.Date,8,2) THEN SUBSTR(eventDeath.Date,8,2) ELSE \"0\" END AS DeathMonth \n" +
                "     , CASE WHEN SUBSTR(eventDeath.Date,10,2) THEN SUBSTR(eventDeath.Date,10,2) ELSE \"0\" END AS DeathdDay \n" +
                "     , CASE WHEN SUBSTR(eventDeath.Date,4,4) THEN SUBSTR(eventDeath.Date,4,4) ELSE \"0\" END AS DeathYear \n" +
                "FROM NameTable name \n" +
                "JOIN PersonTable person \n" +
                "    ON name.OwnerID = person.PersonID \n" +
                "LEFT JOIN EventTable eventBirth ON name.OwnerID = eventBirth.OwnerID AND eventBirth.EventType = 1 \n" +
                "LEFT JOIN EventTable eventDeath \n" +
                "    ON name.OwnerID = eventDeath.OwnerID AND eventDeath.EventType = 2 \n";
            if (JustThisOwnerId != null)
                QUERYNAMES +=
                    $"WHERE name.OwnerID = \"{JustThisOwnerId}\" LIMIT 1";
            string sqlQuery = QUERYNAMES;
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int currentArrayIndex = 0;
            while (reader.Read() && currentArrayIndex < limitListSizeTo)
            {
                var nextName = new Person(
                    arrayIndex: currentArrayIndex,
                    ownerId: reader.GetInt32(0),
                    gender: charToPersonGenderType(reader.GetString(1)[0]),
                    given: reader.GetString(2),
                    surname: reader.GetString(3),
                    birthYear: Int32.Parse(reader.GetString(6)),
                    isLiving: reader.GetBoolean(7),
                    deathYear: Int32.Parse(reader.GetString(10)),
                    generation: generation,
                    xOffset: xOffset,
                    spouseNumber: spouseNumber);

                if (nextName.dataBaseOwnerId == 218)
                    Debug.Log($"We just read in OwnerId {nextName.dataBaseOwnerId}");

                nextName.FixUpDatesForViewing();

                personsList.Add(nextName);
                currentArrayIndex++;
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
            
            PersonGenderType charToPersonGenderType(char sex) =>
                sex.Equals('M') ? PersonGenderType.Male : (sex.Equals('F') ? PersonGenderType.Female : PersonGenderType.NotSet);
        }
    }        
}
