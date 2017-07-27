using System;

namespace battle_simulation
{
    public static class Battle_logger
    {
        public static string battle_log { get; private set; }

        private const string 
            log_table_header = "time;atk;def;aw;dw;ttagd;ahp;dhp;aen;den;"
                             + "aIn;dodged;"
                             + "dIn;next c\n";

        public static void SetFileHeader(Attacker attacker, Defender defender)
        {
            battle_log = ";Attacker;Deffender\n";
            battle_log += ";" + attacker.pokemonName + ";" + defender.pokemonName + "\n";
            battle_log += "qMove;" + attacker.q_move + ";" + defender.q_move + "\n";
            battle_log += ";" + attacker.q_damage + "(" + attacker.q_cd + ")" + ";" + defender.q_damage + "(" + defender.q_cd + ")" + "\n";
            battle_log += "cMove;" + attacker.c_move + ";" + defender.c_move + "\n";
            battle_log += ";" + attacker.c_damage + "(" + attacker.c_cd + ")" + ";" + defender.c_damage + "(" + defender.c_cd + ")" + "\n";
            battle_log += "HP;" + attacker.hp + ";" + (defender.hp * 2).ToString() + "\n";
            battle_log += "use charges;" + attacker.use_charges.ToString() + "\n";
            battle_log += "overkill hp point;" + defender.overkill_hp_point + "\n";
            battle_log += "q dodge window;" + defender.start_dodge_q + ";" + defender.end_dodge_q + "\n";
            battle_log += "c dodge window;" + defender.start_dodge_c + ";" + defender.end_dodge_c + ";;time is subtracted before write\n";
            battle_log += "\n";
        }

        public static void AddTableHeader(string table_name)
        {
            battle_log += table_name + "\n" + log_table_header;
        }

        public static void AddTableSummary(bool win, float time, int atkr_hp_lost, int dfndr_hp_left)
        {
            if (win) battle_log += ";Win;";
            else battle_log += ";Lose;";
            battle_log += "time: " + time + ";";
            battle_log += "Attacker's HP lost: " + atkr_hp_lost + ";";
            battle_log += "Defender's HP left: " + dfndr_hp_left + ";\n\n";
        }

        public static void LogAttackerAction(string action, Attacker attacker, Defender defender, float timer)
        {
            battle_log += Math.Round(timer, 2, MidpointRounding.AwayFromZero) + ";" 
                        + action + ";";
            if (defender.incoming_damage != 0)
                battle_log += "Get Damage";
            battle_log += ";";
            Write_log_line(attacker, defender);
        }
        
        public static void LogDefenderAction(string action, Attacker attacker, Defender defender, float timer)
        {
            battle_log += Math.Round(timer, 1, MidpointRounding.AwayFromZero) + ";;" 
                        + action + ";";
            Write_log_line(attacker, defender);
        }

        public static void LogIncomingDamage(Attacker attacker, Defender defender, float timer)
        {
            battle_log += Math.Round(timer, 1, MidpointRounding.AwayFromZero) + ";GetDamage;;";
            Write_log_line(attacker, defender);
        }

        private static void Write_log_line(Attacker attacker, Defender defender)
        {
            battle_log += Math.Round(attacker.waiting, 2, MidpointRounding.AwayFromZero) + ";"
                        + Math.Round(defender.waiting, 2, MidpointRounding.AwayFromZero) + ";"
                        + Math.Round(attacker.time_til_damage, 2, MidpointRounding.AwayFromZero) + ";"
                        + attacker.current_hp + ";" + defender.current_hp + ";" + attacker.current_en + ";" + defender.current_en + ";"
                        + attacker.incoming_damage + ";" + attacker.dodged + ";"
                        + defender.incoming_damage + ";" + defender.next_attack_is_charged + "\n";
        }
    }
}
