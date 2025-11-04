using System;

namespace RNGtheGame.core
{
    public class NPCBase : Entity
    {
        public string Name { get; set; }
        public int Level { get; set; }

        public NPCBase(string name, int level)
        {
            Name = name;
            Level = level;
        }

        public void Speak(string message)
        {
            Console.WriteLine($"{Name} says: {message}");
        }
    }
}