using System;

namespace battle_simulation
{
    public class Defender
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

        public int current_hp, current_en, incoming_damage;
        public Single waiting;

        //parameters for dodge
        public bool next_attack_is_charged;
        public Single start_dodge_q, end_dodge_q;
        public Single start_dodge_c, end_dodge_c;

        //to handle first defender's attacks with special cool down
        public bool attacked;

        public int overkill_hp_point;

        public bool take_damage()
        {
            current_hp -= incoming_damage;
            current_en += incoming_damage / 2;
            incoming_damage = 0;
            if (current_en > 200) current_en = 200;
            if (current_hp < 1) return true;  //fainted
            return false; //survived
        }

        public void reset()
        {
            current_hp = hp * 2;
            current_en = 0;
            waiting = 0.9F;
            next_attack_is_charged = false;
            incoming_damage = 0;
        }
    }
}
