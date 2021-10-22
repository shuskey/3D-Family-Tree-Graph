﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Enums;
using System;
using Random = UnityEngine.Random;
using Assets.Scripts.DataObjects;
using Assets.Scripts.DataProviders;
using Cinemachine;
using StarterAssets;

public class Tribe : MonoBehaviour
{
	private TribeType tribeType;
	private String rootsMagicFileName;
	private int startingIdForTree;
	private int numberOfGenerations = 5;
	public GameObject personPrefab;
	public GameObject playerControllerPrefab;
	public GameObject playerFollowCameraPrefab;
	public GameObject birthConnectionPrefab;
	public GameObject marriageConnectionPrefab;
	public float marriageEdgepfXScale = 0.4f;
	public GameObject bubblepf;
	public GameObject parentPlatformBirthBubble;
	public GameObject childPlatformReturnToParent;
	public int numberOfPeopleInTribe = 1000;
	public GlobalSpringType globalSpringType = GlobalSpringType.Normal;
	public int generationGap;
	public int spouseGap;
	public int tribeWidth = 200;
	
	private List<PersonNode> gameObjectNodes = new List<PersonNode>();
	private ListOfPersonsFromDataBase myTribeOfPeople;

	const int PlatformChildIndex = 0;

	void Start()
	{
		tribeType = Assets.Scripts.CrossSceneInformation.myTribeType;
		numberOfGenerations = Assets.Scripts.CrossSceneInformation.numberOfGenerations;
		startingIdForTree = Assets.Scripts.CrossSceneInformation.startingDataBaseId;
		rootsMagicFileName = Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath;

		if (tribeType == TribeType.MadeUpData || rootsMagicFileName == null)
		{
			var adam = CreatePersonGameObject("Adam", PersonGenderType.Male, 10, false,60, xOffset:10, generation: 0);

			//SetDemoMotionMode(adam);

			CreatePlayerOnThisPersonObject(adam);

			var eve = CreatePersonGameObject("Eve", PersonGenderType.Female, 10, false, 60, xOffset: 30, generation: 0);

            CreateMarriage(eve, adam, 30);
            var leisha = CreatePersonGameObject("Leisha", PersonGenderType.Female, 1832, false, 1900, generation: 1);
            var bob = CreatePersonGameObject("Bob", PersonGenderType.Male, 1834, false, 1837, generation: 1);
            var grace = CreatePersonGameObject("Grace", PersonGenderType.Female, 1836, false, 1910, generation: 1);
            var olivia = CreatePersonGameObject("Olivia", PersonGenderType.Female, 1837, false, 1920, generation: 1);
            var laura = CreatePersonGameObject("Laura", PersonGenderType.Female, 1840, false, 1930, generation: 1);
            var emily = CreatePersonGameObject("Emily", PersonGenderType.Female, 1842, false, 1940, generation: 1);
            var gary = CreatePersonGameObject("Gary", PersonGenderType.NotSet, 1848, false, 1932, generation: 1);
            AssignParents(leisha, eve, adam);
            AssignParents(bob, eve, adam, ChildRelationshipType.Biological, ChildRelationshipType.Adopted);
            AssignParents(grace, eve, adam);
            AssignParents(olivia, eve, adam);
            AssignParents(laura, eve, adam);
            AssignParents(emily, eve, adam);
            AssignParents(gary, eve, null);

            //var vahe = CreatePersonGameObject("Vahe", PersonGenderType.Male, 1822, false, 1880, generation: 1);
            //CreateMarriage(leisha, vahe, 1840);
            //var kyah = CreatePersonGameObject("Kyah", PersonGenderType.Female, 1841, false, 1980, generation: 2);
            //var hanna = CreatePersonGameObject("Hannah", PersonGenderType.Female, 1843, false, 1960, generation: 2);
            //var jude = CreatePersonGameObject("Jude", PersonGenderType.Male, 1846, false, 1846, generation: 2);
            //var arie = CreatePersonGameObject("Arie", PersonGenderType.Male, 1848, false, 1930, generation: 2);
            //AssignParents(kyah, leisha, vahe);
            //AssignParents(hanna, leisha, vahe);
            //AssignParents(jude, leisha, vahe);
            //AssignParents(arie, leisha, vahe);
        }
		else if (tribeType == TribeType.AllPersons)
		{
            myTribeOfPeople = new ListOfPersonsFromDataBase(rootsMagicFileName);
			myTribeOfPeople.GetListOfPersonsFromDataBase(numberOfPeopleInTribe);

			FixUpDatesBasedOffMarriageDates();

			CreatePersonGameObjectAllPeople();
		}
		else if (tribeType == TribeType.Ancestry)
		{
			myTribeOfPeople = new ListOfPersonsFromDataBase(rootsMagicFileName);
			GetNextLevelOfAncestryForThisPersonIdDataBaseOnly(startingIdForTree, numberOfGenerations, xOffSet: 0.0f, xRange: 1.0f);

			FixUpDatesBasedOffMarriageDates();

			CreatePersonGameObjectForMyTribeOfPeople(startingIdForTree, globalSpringType);

			HookUpTheMarriages();

			NowAddChildrenAssignments();

			PositionTimeBarrier();
		}
		else if (tribeType == TribeType.Descendancy)
        {
			myTribeOfPeople = new ListOfPersonsFromDataBase(rootsMagicFileName);
			GetNextLevelOfDescendancyForThisPersonIdDataBaseOnly(startingIdForTree, numberOfGenerations, xOffSet: 0.0f, xRange: 1.0f);

			FixUpDatesBasedOffMarriageDates();

			CreatePersonGameObjectForMyTribeOfPeople(startingIdForTree, globalSpringType);

			HookUpTheMarriages();

			NowAddChildrenAssignments();

			PositionTimeBarrier();
		}
	}

	void PositionTimeBarrier()
    {
		var timeBarrierObject = GameObject.FindGameObjectsWithTag("TimeBarrier")[0];
		timeBarrierObject.transform.position = new Vector3(tribeWidth / 2f, 0f, DateTime.Now.Year + 0.5f);
		timeBarrierObject.transform.localScale = new Vector3(tribeWidth, 0.1f, tribeWidth);
	}

	void GetNextLevelOfAncestryForThisPersonIdDataBaseOnly(int personId, int depth, float xOffSet, float xRange)
	{
		myTribeOfPeople.GetSinglePersonFromDataBase(personId, generation: depth, xOffSet + xRange / 2, spouseNumber: 0);
		var personWeAreAdding = getPersonForDataBaseOwnerId(personId);

		var listOfFamilyIds = AddParentsAndFixUpDates(personWeAreAdding);
		if (depth == 0)
			return;

		var parentCount = listOfFamilyIds.Count;
		var parentIndex = 0;
		foreach (var familyId  in listOfFamilyIds)
		{
			var newRange = xRange / parentCount;
			var newOffset = xOffSet + parentIndex * newRange;
			GetNextLevelOfAncestryForThisPersonIdDataBaseOnly(familyId, depth - 1, newOffset, newRange);
			parentIndex++;
		}
	}

	void GetNextLevelOfDescendancyForThisPersonIdDataBaseOnly(int personId, int depth, float xOffSet, float xRange)
	{
		myTribeOfPeople.GetSinglePersonFromDataBase(personId, generation: numberOfGenerations - depth, xOffSet + xRange / 2, spouseNumber: 0);
		var personWeAreAdding = getPersonForDataBaseOwnerId(personId);
		var listOfFamilyIds = AddSpousesAndFixUpDates(personWeAreAdding, depth, xOffSet, xRange);
		if (depth == 0)
			return;

		var myChildrenList = new ListOfChildrenFromDataBase(rootsMagicFileName);
		foreach (var familyId  in listOfFamilyIds)
			myChildrenList.GetListOfChildrenFromDataBase(familyId);
			
		var childCount = myChildrenList.childList.Count;
		var childIndex = 0;
		
		foreach (var child in myChildrenList.childList)
		{
			var newRange = xRange / childCount;
			var newOffset = xOffSet + childIndex * newRange;
			GetNextLevelOfDescendancyForThisPersonIdDataBaseOnly(child.childId, depth - 1, newOffset, newRange);
			childIndex++;
		}
	}

	List<int> AddParentsAndFixUpDates(Person forThisPerson)
    {
		var listOfPersonIdsToReturn = new List<int>();
		var myListOfParentages = new ListOfParentsFromDataBase(rootsMagicFileName);
		myListOfParentages.parentList.Clear();
		myListOfParentages.GetListOfParentsFromDataBase(forThisPerson.dataBaseOwnerId);
        foreach (var parentage in myListOfParentages.parentList)
        {
			if (parentage.fatherId != 0)
				listOfPersonIdsToReturn.Add(parentage.fatherId);
			if (parentage.motherId != 0)
				listOfPersonIdsToReturn.Add(parentage.motherId);
		}
		return listOfPersonIdsToReturn;
	}

	List<int> AddSpousesAndFixUpDates(Person forThisPerson, int depth, float xOffset, float xRange)
    {
		var listOfFamilyIdsToReturn = new List<int>();
		var myListOfMarriages = new ListOfMarriagesForPersonFromDataBase(rootsMagicFileName);
		bool thisIsAHusbandQuery = (forThisPerson.gender == PersonGenderType.Male);
		
		myListOfMarriages.GetListOfMarriagesWithEventsForPersonFromDataBase(forThisPerson.dataBaseOwnerId, useHusbandQuery: thisIsAHusbandQuery);
		int spouseNumber = 1;

		foreach (var marriage in myListOfMarriages.marriageList)
		{
			var spouseIdWeAreAdding = thisIsAHusbandQuery ? marriage.wifeId : marriage.husbandId;

			var spouseXOffset = xOffset + (xRange / (myListOfMarriages.marriageList.Count + 2)) * spouseNumber;
			myTribeOfPeople.GetSinglePersonFromDataBase(spouseIdWeAreAdding, generation: numberOfGenerations - depth, spouseXOffset, spouseNumber);
			var spousePersonWeAreAdding = getPersonForDataBaseOwnerId(spouseIdWeAreAdding);
			
			forThisPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear);
			if (spousePersonWeAreAdding != null)
			{
				spousePersonWeAreAdding.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear);
			}
			else
			{
				var errorPersonId = spouseIdWeAreAdding;
			}
			listOfFamilyIdsToReturn.Add(marriage.familyId);
			spouseNumber++;
		}
		return listOfFamilyIdsToReturn;
	}

	void FixUpDatesBasedOffMarriageDates()
    {
		// Lets fixup some more PersonDates based off off Marriage Event Dates
		var myListOfMarriages = new ListOfMarriagesForPersonFromDataBase(rootsMagicFileName);
		foreach (var potentialMarriedPerson in myTribeOfPeople.personsList)
		{
			if (potentialMarriedPerson.gender == PersonGenderType.Male)
			{
				myListOfMarriages.marriageList.Clear();
				myListOfMarriages.GetListOfMarriagesWithEventsForPersonFromDataBase(potentialMarriedPerson.dataBaseOwnerId, useHusbandQuery: true);
				foreach (var marriage in myListOfMarriages.marriageList)
					potentialMarriedPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear);
			}
			else
			{
				myListOfMarriages.marriageList.Clear();
				myListOfMarriages.GetListOfMarriagesWithEventsForPersonFromDataBase(potentialMarriedPerson.dataBaseOwnerId, useHusbandQuery: false);
				foreach (var marriage in myListOfMarriages.marriageList)
					potentialMarriedPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear);
			}
		}
	}

	void CreatePersonGameObjectForMyTribeOfPeople(int startingID, GlobalSpringType globalSpringType = GlobalSpringType.Normal)
    {				
		foreach (var personToAdd in myTribeOfPeople.personsList)
		{
			personToAdd.personNodeGameObject = CreatePersonGameObject(personToAdd, globalSpringType);
            if (personToAdd.dataBaseOwnerId == startingID)
                CreatePlayerOnThisPersonObject(personToAdd.personNodeGameObject);
        }
	}

	void CreatePersonGameObjectAllPeople()
	{
        int counter = 0;
        int generationWidth = (int)Math.Sqrt((double)numberOfPeopleInTribe);
        foreach (var personToAdd in myTribeOfPeople.personsList)
		{
			personToAdd.generation = counter / generationWidth;
			personToAdd.xOffset = ((float)(counter % generationWidth) / (float)generationWidth) / 1000f; //Random.value;
			personToAdd.personNodeGameObject = CreatePersonGameObject(personToAdd);
            counter++;
        }
	}

	void HookUpTheMarriages()
	{
		// time to hook up some marriages
		var myListOfMarriages = new ListOfMarriagesForPersonFromDataBase(rootsMagicFileName);
		foreach (var potentialHusbandPerson in myTribeOfPeople.personsList)
		{
			if (potentialHusbandPerson.gender == PersonGenderType.Male)
			{
				myListOfMarriages.marriageList.Clear();
				myListOfMarriages.GetListOfMarriagesWithEventsForPersonFromDataBase(potentialHusbandPerson.dataBaseOwnerId);
				foreach (var marriage in myListOfMarriages.marriageList)
				{
					int marriageYearToUse = potentialHusbandPerson.FixUpAndReturnMarriageDate(marriage.marriageYear);
					bool divorcedOrAnnuledFlag = false;
					int divorcedOrAnnuledDate = 0;

					if (marriage.divorcedYear != 0 || marriage.annulledYear != 0)
                    {
						divorcedOrAnnuledFlag = true;
						if (marriage.divorcedYear != 0)
							divorcedOrAnnuledDate = marriage.divorcedYear;
						if (marriage.annulledYear != 0 && marriage.annulledYear < marriage.divorcedYear)
							divorcedOrAnnuledDate = marriage.annulledYear;
                    }
					CreateMarriage(
						getGameObjectForDataBaseOwnerId(marriage.wifeId),
						getGameObjectForDataBaseOwnerId(marriage.husbandId),
						marriageYearToUse,
						divorcedOrAnnuledFlag,
						divorcedOrAnnuledDate);
				}
			}
		}
	}

	void NowAddChildrenAssignments()
	{ 
		// first came love, then came marriage, then came a baby in a baby carriage
		var myListOfParents = new ListOfParentsFromDataBase(rootsMagicFileName);
		foreach (var child in myTribeOfPeople.personsList)
		{
			myListOfParents.parentList.Clear();
			myListOfParents.GetListOfParentsFromDataBase(child.dataBaseOwnerId);
			foreach (var myParents in myListOfParents.parentList)
			{
				AssignParents(
					getGameObjectForDataBaseOwnerId(child.dataBaseOwnerId),
					getGameObjectForDataBaseOwnerId(myParents.motherId),
					getGameObjectForDataBaseOwnerId(myParents.fatherId),
					myParents.relationToMother,
					myParents.relationToFather);
			}
		}
	}

	GameObject getGameObjectForDataBaseOwnerId(int ownerId) =>
				myTribeOfPeople.personsList.Find(x => x.dataBaseOwnerId == ownerId)?.personNodeGameObject;
	Person getPersonForDataBaseOwnerId(int ownerId) =>
				myTribeOfPeople.personsList.Find(x => x.dataBaseOwnerId == ownerId);

	GameObject CreatePersonGameObject(string name, PersonGenderType personGender, int birthEventDate,
		bool isLiving = true, int deathEventDate = 0, int generation = 0, float xOffset = 0.0f,
		int spouseNumber = 0,
		int originalBirthDate = 0, int originalDeathDate = 0, string dateQualityInformationString = "",
		int databaseOwnerArry = 0, int tribeArrayIndex = 0, GlobalSpringType globalSpringType = GlobalSpringType.Normal,
		Person person = null)
    {
		var currentYear = DateTime.Now.Year;
	
		var age = isLiving ? currentYear - birthEventDate : deathEventDate - birthEventDate;

		var x = xOffset * tribeWidth;
		var y = generation * generationGap + spouseNumber * spouseGap;
		
		var newPersonGameObject = Instantiate(personPrefab, new Vector3(x, y, birthEventDate), Quaternion.identity);		
		newPersonGameObject.transform.parent = transform;
		newPersonGameObject.name = name;
		var personObjectScript = newPersonGameObject.GetComponent<PersonNode>();

		personObjectScript.SetIndexes(databaseOwnerArry, tribeArrayIndex, person);
		personObjectScript.SetLifeSpan(birthEventDate, age, isLiving);
		personObjectScript.AddDateQualityInformation((birthEventDate, originalBirthDate), (deathEventDate, originalDeathDate), dateQualityInformationString);
		personObjectScript.SetPersonGender(personGender);
		personObjectScript.SetEdgePrefab(birthConnectionPrefab, marriageConnectionPrefab, bubblepf, parentPlatformBirthBubble, childPlatformReturnToParent, marriageEdgepfXScale);
		personObjectScript.addMyBirthQualityBubble();
		personObjectScript.SetGlobalSpringType(globalSpringType);

		//TODO use gender to set the color of the platform	
		//
		return newPersonGameObject;
	}

	void SetDemoMotionMode(GameObject personGameObject)
    {
		personGameObject.GetComponent<PersonNode>().SetDebugAddMotionSetting(true);
	}

	GameObject CreatePlayerOnThisPersonObject(GameObject personGameObject)
    {
		personGameObject.GetComponent<Rigidbody>().isKinematic = true;  // Lets make this on stay put

		GameObject playerGameObject = Instantiate(playerControllerPrefab);

		//playerGameObject.transform.SetParent(personGameObject.transform, false);
		
		//playerGameObject.transform.position = new Vector3(0f, 1f, 0f);

		//playerGameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

		GameObject[] targets = GameObject.FindGameObjectsWithTag("CinemachineTarget");
		GameObject target = targets.FirstOrDefault(t => t.transform.IsChildOf(playerGameObject.transform));
		
		CreatePlayerFollowCameraObject(target);

		var thirdPersonContollerScript = playerGameObject.GetComponent<ThirdPersonController>();
		thirdPersonContollerScript.TeleportTo(personGameObject.transform, new Vector3(0,0.5f,0), ticksToHoldHere: 100);

		return playerGameObject;
    }

	private void CreatePlayerFollowCameraObject(GameObject target)
	{
		var playerFollowCameraGameObject = Instantiate(playerFollowCameraPrefab);

		var vCam = playerFollowCameraGameObject.GetComponent<CinemachineVirtualCamera>();
		vCam.Follow = target.transform;
		
	}
		

GameObject CreatePersonGameObject(Person person, GlobalSpringType globalSpringType = GlobalSpringType.Normal)
	{
		return CreatePersonGameObject($"{person.givenName} {person.surName}", person.gender, person.birthEventDate,
			person.isLiving, person.deathEventDate, person.generation, person.xOffset, person.spouseNumber,
			person.originalBirthEventDate, person.originalDeathEventDate,
			person.dateQualityInformationString,
			person.dataBaseOwnerId, person.tribeArrayIndex, globalSpringType,
			person);
	}

	void CreateMarriage(GameObject wifePerson, GameObject husbandPerson, int marriageEventDate, bool divorcedFlag= false,int divorcedEventDate=0)
    {
		// We may not have loaded a full set of family information
		// If the husband or wife is not found, skip the marriage
		if (ReferenceEquals(wifePerson, null)
			|| ReferenceEquals(husbandPerson, null))
			return;
		var husbandPersonNode = husbandPerson.GetComponent<PersonNode>();
		var wifePersonNode = wifePerson.GetComponent<PersonNode>();
		var wifeAge = wifePersonNode.lifeSpan;

		// We have some married people with no birthdates
		var wifeAgeAtMarriage = (float)(marriageEventDate - wifePersonNode.birthDate);	
		var husbandAge = husbandPersonNode.lifeSpan;
		// We have some married people with no birthdates
		var husbandAgeAtMarriage = (float)(marriageEventDate - husbandPersonNode.birthDate);
		// TODO does not work for divorcedEventDate = 0
		var marriageLength = divorcedFlag ?
			divorcedEventDate - marriageEventDate : (int)Mathf.Min(wifePersonNode.birthDate + wifeAge, husbandPersonNode.birthDate + husbandAge) - marriageEventDate;
		// Just in case birthdate and ages are zero
		if (marriageLength < 0)
			marriageLength = 1;

		var wifeMarriageConnectionPointPercent = wifeAge != 0f ? wifeAgeAtMarriage / wifeAge : 0.5f;
		var husbandMarriageConnectionPointPercent = husbandAge != 0f ? husbandAgeAtMarriage / husbandAge: 0.5f; 
		
		wifePersonNode.AddMarriageEdge(
			husbandPersonNode, 
			wifeMarriageConnectionPointPercent, 
			husbandMarriageConnectionPointPercent, 
			marriageEventDate,
			marriageLength);
	}

	void AssignParents(GameObject childPerson, GameObject motherPerson, GameObject fatherPerson,
		ChildRelationshipType motherChildRelationshipType = ChildRelationshipType.Biological,
		ChildRelationshipType fatherChildRelationshipType = ChildRelationshipType.Biological)
    {
		var childPersonNode = childPerson.GetComponent<PersonNode>();

		if (motherPerson != null && !motherPerson.Equals(null))
		{
			var motherPersonNode = motherPerson.GetComponent<PersonNode>();
			var motherAge = motherPersonNode.lifeSpan;
			var motherAgeAtChildBirth = (float)(childPersonNode.birthDate - motherPersonNode.birthDate);
			motherPersonNode.AddBirthEdge(childPersonNode, motherAgeAtChildBirth / motherAge, motherChildRelationshipType, childPersonNode.birthDate);
		}

		if (fatherPerson != null && !fatherPerson.Equals(null))
		{
			var fatherPersonNode = fatherPerson.GetComponent<PersonNode>();
			var fatherAge = fatherPersonNode.lifeSpan;
			var fatherAgeAtChildBirth = (float)(childPersonNode.birthDate - fatherPersonNode.birthDate);
			fatherPersonNode.AddBirthEdge(childPersonNode, fatherAgeAtChildBirth / fatherAge, fatherChildRelationshipType, childPersonNode.birthDate);
		}
	}

	void Update() { }

}