using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomData
{
    public string roomName;
    public int capacity;
}

[System.Serializable]
public class Reservation
{
    public string roomName;
    public string date;
    public int startHour; 
    public int endHour;   
    public string description;
    public int amountOfPeople;
    public bool isPlayerBooking; 
}

public class BookingManager : MonoBehaviour
{
    public static BookingManager Instance; 

    [Header("Simulation Settings")]
    [Tooltip("How many computer bookings should spawn at the start?")]
    public int numberOfRandomBookings = 5;
    [Tooltip("The date these random bookings will spawn on")]
    public string defaultDate = "11 mei 2026";

    public List<Reservation> allReservations = new List<Reservation>();
    
    public List<RoomData> rooms = new List<RoomData>
    {
        new RoomData { roomName = "Kamer 10", capacity = 6 },
        new RoomData { roomName = "Kamer 11", capacity = 8 },
        new RoomData { roomName = "Kamer 30", capacity = 15 },
        new RoomData { roomName = "Kamer 32", capacity = 6 },
        new RoomData { roomName = "Kamer 34", capacity = 8 },
        new RoomData { roomName = "Kamer 35", capacity = 15 },
        new RoomData { roomName = "Kamer 5", capacity = 6 },
        new RoomData { roomName = "Kamer 7a", capacity = 8 }
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        GenerateComputerBookings();
    }

    void GenerateComputerBookings()
    {
        // A list of random names to make the simulation look real
        string[] meetingNames = { "Overleg", "Inzicht in project", "Teamvergadering", "Klantgesprek", "Brainstorm", "Lunchbespreking" };

        int spawned = 0;
        int safetyNet = 0; // Prevents the game from freezing if it can't find a free spot

        while (spawned < numberOfRandomBookings && safetyNet < 100)
        {
            safetyNet++;

            // 1. Pick a random room
            RoomData randomRoom = rooms[Random.Range(0, rooms.Count)];

            // 2. Pick a random time between 9:00 and 16:00
            int startH = Random.Range(9, 16); 
            
            // 3. Pick a random duration (1 or 2 hours)
            int duration = Random.Range(1, 3); 
            int endH = Mathf.Min(startH + duration, 17); // Make sure it doesn't go past 17:00

            // 4. If the slot is actually free, create the booking!
            if (IsSlotFree(randomRoom.roomName, defaultDate, startH, endH))
            {
                string randomDesc = meetingNames[Random.Range(0, meetingNames.Length)];
                int randomPeople = Random.Range(2, randomRoom.capacity + 1);

                allReservations.Add(new Reservation 
                { 
                    roomName = randomRoom.roomName, 
                    date = defaultDate, 
                    startHour = startH, 
                    endHour = endH, 
                    description = randomDesc, 
                    amountOfPeople = randomPeople, 
                    isPlayerBooking = false 
                });

                spawned++; // Successfully added one, increase the count
            }
        }
    }

    public void AddPlayerBooking(string room, string date, int start, int end, string desc, int people)
    {
        allReservations.Add(new Reservation { roomName = room, date = date, startHour = start, endHour = end, description = desc, amountOfPeople = people, isPlayerBooking = true });
    }

    public void DeleteBooking(Reservation resToDelete)
    {
        if (allReservations.Contains(resToDelete))
        {
            allReservations.Remove(resToDelete);
        }
    }

    public bool IsSlotFree(string room, string date, int newStart, int newEnd)
    {
        foreach (Reservation res in allReservations)
        {
            if (res.roomName == room && res.date == date)
            {
                if (newStart < res.endHour && newEnd > res.startHour) return false; 
            }
        }
        return true; 
    }

    public Reservation GetBookingStartingAt(string room, string date, int hour)
    {
        foreach (Reservation res in allReservations)
        {
            if (res.roomName == room && res.date == date && res.startHour == hour) return res;
        }
        return null;
    }

    public int GetRoomCapacity(string name)
    {
        foreach (var room in rooms) { if (room.roomName == name) return room.capacity; }
        return 0;
    }
}