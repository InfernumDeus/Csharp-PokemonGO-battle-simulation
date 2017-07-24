using System;

namespace battle_simulation
{
    public class Attacker
    {
        public string pokemonName;
        public int attack, defense, hp;
        public Single cpm;

        public string q_move, c_move;
        public int q_power, c_power;
        public Single q_cd, c_cd;
        public Single q_mult, c_mult;
        public int q_damage, c_damage;
        public int enrg_gain, enrg_cost;

        public bool use_charges;

        public int current_hp, current_en, incoming_damage;
        public Single waiting;
        
        //never by default
        public Single time_til_damage = 3.4F * (float)Math.Pow(10, 37);

        public bool dodged; //to don't dodge already dodged attack

        public bool take_damage()
        {
            dodged = false;
            current_hp -= incoming_damage;
            current_en += incoming_damage / 2;
            incoming_damage = 0;
            if (current_en > 100) current_en = 100;
            if (current_hp < 1) return true;  //fainted
            return false; //survived
        }

        public void reset()
        {
            current_hp = hp;
            current_en = 0;
            waiting = 0;
            dodged = false;
            incoming_damage = 0;
            time_til_damage = 3.4F * (float)Math.Pow(10, 37);
        }
    }
}
