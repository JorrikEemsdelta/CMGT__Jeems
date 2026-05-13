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
    public string bookerName; 
    public bool isPlayerBooking; 
}

public class BookingManager : MonoBehaviour
{
    public static BookingManager Instance; 

    [Header("Simulation Settings")]
    public int bookingsPerDay = 6;
    
    public string[] weekDates = { 
        "11 mei 2026 (Ma)", 
        "12 mei 2026 (Di)", 
        "13 mei 2026 (Wo)", 
        "14 mei 2026 (Do)", 
        "15 mei 2026 (Vr)" 
    };

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
        string[] meetingNames = { "Overleg", "Inzicht in project", "Teamvergadering", "Klantgesprek", "Brainstorm", "Lunchbespreking", "Sollicitatie" };
        string[] fakeNames = { "Jan de Vries", "Sanne Bakker", "Pieter Visser", "Lisa Smit", "Tom de Boer", "Maaike Dijk", "Klaas Jansen" };

        foreach (string currentDate in weekDates)
        {
            int spawned = 0;
            int safetyNet = 0;

            while (spawned < bookingsPerDay && safetyNet < 100)
            {
                safetyNet++;
                RoomData randomRoom = rooms[Random.Range(0, rooms.Count)];
                int startH = Random.Range(9, 16); 
                int duration = Random.Range(1, 3); 
                int endH = Mathf.Min(startH + duration, 17); 

                if (IsSlotFree(randomRoom.roomName, currentDate, startH, endH))
                {
                    string randomDesc = meetingNames[Random.Range(0, meetingNames.Length)];
                    string randomBooker = fakeNames[Random.Range(0, fakeNames.Length)]; 
                    int randomPeople = Random.Range(2, randomRoom.capacity + 1);

                    allReservations.Add(new Reservation 
                    { 
                        roomName = randomRoom.roomName, 
                        date = currentDate, 
                        startHour = startH, 
                        endHour = endH, 
                        description = randomDesc, 
                        amountOfPeople = randomPeople, 
                        bookerName = randomBooker, 
                        isPlayerBooking = false 
                    });
                    spawned++; 
                }
            }
        }
    }

    public void AddPlayerBooking(string room, string date, int start, int end, string desc, int people)
    {
        allReservations.Add(new Reservation 
        { 
            roomName = room, 
            date = date, 
            startHour = start, 
            endHour = end, 
            description = desc, 
            amountOfPeople = people, 
            bookerName = "Jij", 
            isPlayerBooking = true 
        });

        // --- UPDATED: Send the exact 'end' time instead of the duration ---
        if (AssignmentManager.Instance != null)
        {
            AssignmentManager.Instance.CheckActionBooking(date, start, end, people);
        }
        else
        {
            Debug.LogWarning("AssignmentManager is missing in the scene! Cannot check assignments yet.");
        }
    }

    public void DeleteBooking(Reservation resToDelete)
    {
        if (allReservations.Contains(resToDelete)) allReservations.Remove(resToDelete);
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