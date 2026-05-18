using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class BookingSaveData
{
    public List<Reservation> savedReservations;
}

[System.Serializable]
public class RoomData
{
    public string roomName;
    public int capacity;
}

[System.Serializable]
public class Reservation
{
    public bool isPlayerBooking;
    public string roomName;
    public string date;
    public int startHour;
    public int endHour;
    public string bookerName;
    public string description;
    public int amountOfPeople;
}

public class BookingManager : MonoBehaviour
{
    public static BookingManager Instance;

    [Header("Setup Data")]
    public List<RoomData> rooms = new List<RoomData>();
    [Tooltip("The list of dates used in the dropdown (e.g. 11 mei 2026 (Ma))")]
    public string[] weekDates;

    [Header("Random Generator Settings")]
    [Range(0, 10)]
    public int reservationsPerDay = 3;

    [Header("Active Database (Live Data)")]
    public List<Reservation> allReservations = new List<Reservation>();

    // Random data for the generator to pick from
    private string[] randomNames = { "S. Bakker", "M. Jansen", "J. de Vries", "L. Visser", "P. Smits", "E. Mulder" };
    private string[] randomDescs = { "Overleg", "Meeting", "Project focus", "Interne audit", "Brainstorm", "Koffie break" };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadData(); 
    }

    // ==========================================
    // HELPER FUNCTIONS
    // ==========================================

    public int GetRoomCapacity(string roomName)
    {
        foreach (var room in rooms)
        {
            if (room.roomName == roomName) return room.capacity;
        }
        return 0; 
    }

    public Reservation GetBookingStartingAt(string roomName, string date, int hour)
    {
        foreach (var res in allReservations)
        {
            if (res.roomName == roomName && res.date == date)
            {
                if (hour >= res.startHour && hour < res.endHour) return res;
            }
        }
        return null;
    }

    // ==========================================
    // ADD / DELETE LOGIC
    // ==========================================

    public void AddPlayerBooking(Reservation res)
    {
        res.isPlayerBooking = true;
        allReservations.Add(res);
        SaveData(); 
    }

    public void AddPlayerBooking(string room, string date, int start, int end, string desc, int people)
    {
        Reservation newRes = new Reservation
        {
            isPlayerBooking = true,
            roomName = room,
            date = date,
            startHour = start,
            endHour = end,
            bookerName = "Jij (Speler)", 
            description = desc,
            amountOfPeople = people
        };
        allReservations.Add(newRes);
        SaveData(); 
    }

    public void DeleteBooking(Reservation res)
    {
        if (allReservations.Contains(res))
        {
            allReservations.Remove(res);
            SaveData();
        }
    }

    public bool IsSlotFree(string room, string date, int start, int end)
    {
        foreach (var res in allReservations)
        {
            if (res.roomName == room && res.date == date)
            {
                if (start < res.endHour && end > res.startHour) return false;
            }
        }
        return true;
    }

    // ==========================================
    // THE SAVE SYSTEM
    // ==========================================

    public void SaveData()
    {
        BookingSaveData data = new BookingSaveData();
        data.savedReservations = allReservations;
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("BookingSaveData", json);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("BookingSaveData"))
        {
            string json = PlayerPrefs.GetString("BookingSaveData");
            BookingSaveData data = JsonUtility.FromJson<BookingSaveData>(json);
            allReservations = data.savedReservations ?? new List<Reservation>();
        }
    }

    public void ClearSaveData()
    {
        allReservations.RemoveAll(res => res.isPlayerBooking);
        PlayerPrefs.DeleteKey("BookingSaveData");
        PlayerPrefs.Save();
    }

    // ==========================================
    // RANDOM DUMMY DATA GENERATOR
    // ==========================================
    public void GenerateDummyBookings()
    {
        // 1. Clear current non-player bookings
        allReservations.RemoveAll(res => !res.isPlayerBooking);

        if (rooms.Count == 0 || weekDates.Length == 0)
        {
            Debug.LogError("Setup Data (Rooms/Dates) missing on BookingManager!");
            return;
        }

        // 2. Loop through every day in your week
        foreach (string date in weekDates)
        {
            int createdToday = 0;
            int attempts = 0;

            // Try to create the requested amount of bookings
            while (createdToday < reservationsPerDay && attempts < 50)
            {
                attempts++;

                // Pick random room and time
                RoomData randomRoom = rooms[Random.Range(0, rooms.Count)];
                int start = Random.Range(9, 16); // 09:00 to 16:00
                int duration = Random.Range(1, 3); // 1 or 2 hours
                int end = start + duration;

                // Check if the random slot is actually free
                if (IsSlotFree(randomRoom.roomName, date, start, end))
                {
                    Reservation randomRes = new Reservation
                    {
                        isPlayerBooking = false,
                        roomName = randomRoom.roomName,
                        date = date,
                        startHour = start,
                        endHour = end,
                        bookerName = randomNames[Random.Range(0, randomNames.Length)],
                        description = randomDescs[Random.Range(0, randomDescs.Length)],
                        amountOfPeople = Random.Range(1, randomRoom.capacity + 1)
                    };

                    allReservations.Add(randomRes);
                    createdToday++;
                }
            }
        }

        SaveData(); 
        Debug.Log($"✅ Succes: {reservationsPerDay} willekeurige boekingen per dag gegenereerd!");
    }
}