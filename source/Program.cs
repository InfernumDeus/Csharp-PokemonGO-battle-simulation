/* Pokemon GO IV and level calculator
created: 21.07.2017
author: InfernumDeus
e-mail: sataha19@mail.ru
github: https://github.com/InfernumDeus

Feel free to use it in your projects and modify in any way.*/

using System;
using System.IO;
using System.Data;
using System.Collections.Generic;

//ExcelDataReader
//https://github.com/ExcelDataReader/ExcelDataReader
using Excel;

using CPmult;

namespace battle_simulation {
    class QMove
    {
        public string name, type;
        public int power, energy_gain;
        public float cd, dmg_start;
    }

    class CMove
    {
        public string name, type;
        public int power, energy_cost;
        public float cd, dmg_start;
    }

    public class Battle_Result
    {
        public bool use_charges;

        public bool no_dodge_result;
        public int no_dodge_hp;
        public int no_dodge_enemy_hp;
        public float no_dodge_time;

        public bool ch_dodge_result;
        public int ch_dodge_hp;
        public int ch_dodge_enemy_hp;
        public float ch_dodge_time;

        public bool full_dodge_result;
        public int full_dodge_hp;
        public int full_dodge_enemy_hp;
        public float full_dodge_time;

        public Battle_Result() {
            no_dodge_time = -1;
            ch_dodge_time = -1;
            full_dodge_time = -1;
        }

        public override string ToString() {
            return "Charges = " + use_charges + "\r\n\r\n"

                 + "No dodge\r\n"
                 + "Result = " + no_dodge_result + "\r\n"
                 + "Attacker's HP lost = " + no_dodge_hp + "\r\n"
                 + "Defender's HP left = " + no_dodge_enemy_hp + "\r\n"
                 + "Time = " + no_dodge_time + "\r\n\r\n"

                 + "Charge dodge\r\n"
                 + "Result = " + ch_dodge_result + "\r\n"
                 + "Attacker's HP lost = " + ch_dodge_hp + "\r\n"
                 + "Defender's HP left = " + ch_dodge_enemy_hp + "\r\n"
                 + "Time = " + ch_dodge_time + "\r\n\r\n"

                 + "Full dodge\r\n"
                 + "Result = " + full_dodge_result + "\r\n"
                 + "Attacker's HP lost = " + full_dodge_hp + "\r\n"
                 + "Defender's HP left = " + full_dodge_enemy_hp + "\r\n"
                 + "Time = " + full_dodge_time + "\r\n\r\n";
        }
    }

    class Battle_simulator
    {
        //STIMULATION OPTIONS//
        private static bool dodge_all_quick_moves;
        private static bool dodge_all_charge_moves;
        private static readonly float user_reaction_constant = 0.4F; //time between yellow flash and dodge

        private static readonly bool write_battle_log = true;

        //with global_hp_rate = 100, defender and attacker will heve 100 times more hp 
        //time limit of battle would be 9900 sec
        //and after simulation resulting time  would be divided by 100
        //default value = 1
        public static int global_hp_rate = 1;
        //STIMULATION OPTIONS//

        private static float timer;

        private static Attacker attacker = new Attacker();
        private static Defender defender = new Defender();

        private static bool delayed_charge = false;
        
        private static DataTable typeTable;
        private static DataTable pokemonTable;
        private static DataTable nTable;
        private static DataTable sTable;

        private static List<QMove> qMovesList = new List<QMove>();
        private static List<CMove> cMovesList = new List<CMove>();
        
        private static string battle_log = "";        
        private static void write_log_line(Attacker attacker, Defender defender)
        {
            battle_log += Math.Round(attacker.waiting, 2, MidpointRounding.AwayFromZero) + ";"
                        + Math.Round(defender.waiting, 2, MidpointRounding.AwayFromZero) + ";"
                        + Math.Round(attacker.time_til_damage, 2, MidpointRounding.AwayFromZero) + ";"
                        + attacker.current_hp + ";" + defender.current_hp + ";" + attacker.current_en + ";" + defender.current_en + ";"
                        + attacker.incoming_damage.ToString() + ";" + attacker.dodged.ToString() + ";"
                        + defender.incoming_damage.ToString() + ";" + defender.next_attack_is_charged.ToString() + "\n";
        }

        private static int NumToDamageRatio (string cellValue) {
            switch (cellValue) {
                case "0" : return 6400;
                case "1" : return 8000;
                case "3" : return 12500;
                case "4" : return 15625;
                default  : return 10000;
            }
        }

        //number of rows in data tables
        static int typeLimit = 61;
        static int pokemonLimit = 238;
        static int nLimit = 76;
        static int sLimit = 115;

        static void Main(string[] args) {
            string filePath = Directory.GetCurrentDirectory() + "\\data2.1.xlsx";
            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            DataSet result = excelReader.AsDataSet();
            
            typeTable    = result.Tables[0];
            pokemonTable = result.Tables[1];
            nTable       = result.Tables[2];
            sTable       = result.Tables[3];
            
            typeTable.Columns[0].ColumnName  = "type";
            typeTable.Columns[1].ColumnName  = "nor";
            typeTable.Columns[2].ColumnName  = "fir";
            typeTable.Columns[3].ColumnName  = "wat";
            typeTable.Columns[4].ColumnName  = "ele";
            typeTable.Columns[5].ColumnName  = "gra";
            typeTable.Columns[6].ColumnName  = "ice";
            typeTable.Columns[7].ColumnName  = "fig";
            typeTable.Columns[8].ColumnName  = "poi";
            typeTable.Columns[9].ColumnName  = "gro";
            typeTable.Columns[10].ColumnName = "fly";
            typeTable.Columns[11].ColumnName = "psy";
            typeTable.Columns[12].ColumnName = "bug";
            typeTable.Columns[13].ColumnName = "roc";
            typeTable.Columns[14].ColumnName = "gho";
            typeTable.Columns[15].ColumnName = "dra";
            typeTable.Columns[16].ColumnName = "dar";
            typeTable.Columns[17].ColumnName = "ste";
            typeTable.Columns[18].ColumnName = "fai";
            
            for (int i = 1; i < nLimit; i++) {
                QMove q = new QMove();
                q.name = nTable.Rows[i][1].ToString().Trim();
                q.type = nTable.Rows[i][2].ToString().Trim();
                q.power = Convert.ToInt32(nTable.Rows[i][3]);
                q.cd = Convert.ToSingle(nTable.Rows[i][4]);
                q.dmg_start = Convert.ToSingle(nTable.Rows[i][5]);
                q.energy_gain = Convert.ToInt32(nTable.Rows[i][6]);
                qMovesList.Add(q);
            }
            
            for (int i = 1; i < sLimit; i++) { 
                CMove c = new CMove();
                c.name = sTable.Rows[i][1].ToString().Trim();
                c.type = sTable.Rows[i][2].ToString().Trim();
                c.power = Convert.ToInt32(sTable.Rows[i][3]);
                c.cd = Convert.ToSingle(sTable.Rows[i][4]);
                c.dmg_start = Convert.ToSingle(sTable.Rows[i][5]);
                c.energy_cost = Convert.ToInt32(sTable.Rows[i][6]);
                cMovesList.Add(c);
            }
            
            set_battle_options("Snorlax/Lick/Hyper Beam/27.5/13/15/14",
                               "Rhydon/Rock Smash/Stone Edge/24/14/14/7");

            Simulate_battles();

            filePath = Directory.GetCurrentDirectory() + "\\battle log.csv";

            File.WriteAllText(filePath, battle_log);
            return;
        }

        private static Battle_Result Simulate_battles()
        {
            if (write_battle_log) Battle_logger.SetFileHeader(attacker, defender);
                       

            Battle_Result battle_result = new Battle_Result();
            Tuple<bool, float, int, int> br;

            //Console.WriteLine("Don't dodge");
            if (write_battle_log) Battle_logger.AddTableHeader("no dodge");

            dodge_all_quick_moves = false;
            dodge_all_charge_moves = false;
            br = battle();
            battle_result.no_dodge_result = br.Item1;
            battle_result.no_dodge_time = br.Item2;
            battle_result.no_dodge_hp = br.Item3;
            battle_result.no_dodge_enemy_hp = br.Item4;
            
            if (write_battle_log) Battle_logger.AddTableSummary(br.Item1, br.Item2, br.Item3, br.Item4);

            ////console.WriteLine("------------");
            ////console.WriteLine();

            //Console.WriteLine("Dodge charged moves");
            if (write_battle_log) Battle_logger.AddTableHeader("charge dodge");

            dodge_all_charge_moves = true;
            br = battle();
            battle_result.ch_dodge_result = br.Item1;
            battle_result.ch_dodge_time = br.Item2;
            battle_result.ch_dodge_hp = br.Item3;
            battle_result.ch_dodge_enemy_hp = br.Item4;

            if (write_battle_log) Battle_logger.AddTableSummary(br.Item1, br.Item2, br.Item3, br.Item4);

            ////console.WriteLine("------------");
            ////console.WriteLine();

            //Console.WriteLine("Dodge all moves");
            if (write_battle_log) Battle_logger.AddTableHeader("full dodge");
            dodge_all_quick_moves = true;
            br = battle();
            battle_result.full_dodge_result = br.Item1;
            battle_result.full_dodge_time = br.Item2;
            battle_result.full_dodge_hp = br.Item3;
            battle_result.full_dodge_enemy_hp = br.Item4;

            if (write_battle_log) Battle_logger.AddTableSummary(br.Item1, br.Item2, br.Item3, br.Item4);

            battle_result.use_charges = attacker.use_charges;
            return battle_result;
        }

        //output: win/time/attacker's hp LOST/defender's hp LEFT
        private static Tuple<bool, float, int, int> battle()
        {
            try
            {
                attacker.reset();
                defender.reset();
                timer = 0.6F; //the moment attacker do first move

                float[] bufAr = new float[4];
                bufAr[0] = defender.start_dodge_q;
                bufAr[1] = defender.end_dodge_q;
                bufAr[2] = defender.start_dodge_c;
                bufAr[3] = defender.end_dodge_c;

                //first attack comes in 1.5s
                defender.incoming_damage = attacker.q_damage;
                attacker.waiting = attacker.q_cd;
                defender.attacked = false;
                while (!defender.attacked)
                    hit();

                //second is after 1s
                defender.start_dodge_q = 0.6F; //-0.4F
                defender.end_dodge_c = 0.3F; //-0.7F

                defender.waiting = 1;
                attacker.time_til_damage = 1;
                defender.attacked = false;
                while (!defender.attacked)
                    hit();

                //third is after 2.5s
                defender.start_dodge_q = 2.1F; //-0.4F
                defender.end_dodge_q = 1.8F; //-0.7F

                defender.waiting = 2.5F;
                defender.attacked = false;
                if (attacker.time_til_damage > 2.5F)
                    attacker.time_til_damage = 2.5F;
                while (!defender.attacked)
                    hit();
                
                defender.start_dodge_q = bufAr[0];
                defender.end_dodge_q = bufAr[1];
                defender.start_dodge_c = bufAr[2];
                defender.end_dodge_c = bufAr[3];

                //rest of the battle
                while (true)
                    if (timer < (99.9F * global_hp_rate))
                        hit();
                    else throw new Exception("2");
            }
            catch (Exception ex)
            {
                if (ex.Message == "1")
                {
                    return Tuple.Create<bool, float, int, int>(
                        true,
                        timer / (float)global_hp_rate,
                        (int)(((float)attacker.hp - (float)attacker.current_hp) / (float)global_hp_rate), 
                        (int)((float)defender.current_hp / (float)global_hp_rate));
                }
                else if (ex.Message == "2")
                {
                    return Tuple.Create<bool, float, int, int>(
                        false,
                        timer / (float)global_hp_rate,
                        (int)(((float)attacker.hp - (float)attacker.current_hp) / (float)global_hp_rate),
                        (int)((float)defender.current_hp / (float)global_hp_rate));
                }
                else throw ex;
            }
        }

        //maybe here is some way to optimise dodge checks
        private static void hit()
        {
            if (attacker.waiting < defender.waiting &&
                attacker.waiting < attacker.time_til_damage)
            {   //attacker acts
                timer += attacker.waiting;
                defender.waiting -= attacker.waiting;
                attacker.time_til_damage -= attacker.waiting;

                //console.WriteLine("Defender get hit");
                if (defender.take_damage())
                    throw new Exception("1"); //attacker wins
                
                if (dodge_all_quick_moves &&
                    !attacker.dodged &&
                    !defender.next_attack_is_charged &&
                    defender.waiting <= defender.start_dodge_q &&
                    defender.waiting > defender.end_dodge_q &&
                    attacker.incoming_damage > 0)
                {
                    //console.WriteLine("Dodge");
                    if (write_battle_log)
                        Battle_logger.LogAttackerAction("Dodge", attacker, defender, timer);

                    attacker.dodged = true;
                    defender.incoming_damage = 0;
                    attacker.waiting = 1;
                    attacker.incoming_damage = (int)Math.Floor(attacker.incoming_damage / 4F);
                    return;
                }
                else 
                if (dodge_all_charge_moves &&
                    !attacker.dodged &&
                    defender.next_attack_is_charged &&
                    defender.waiting <= defender.start_dodge_c &&
                    defender.waiting > defender.end_dodge_c &&
                    attacker.incoming_damage > 0)
                {
                    //console.WriteLine("Dodge");
                    if (write_battle_log)
                        Battle_logger.LogAttackerAction("Dodge", attacker, defender, timer);

                    attacker.dodged = true;
                    defender.incoming_damage = 0;
                    attacker.waiting = 1;
                    attacker.incoming_damage = (int)Math.Floor(attacker.incoming_damage / 4F);
                    return;
                }
                
                if (attacker.enrg_cost <= attacker.current_en &&
                    defender.current_hp > defender.overkill_hp_point)
                { //c move
                    //console.WriteLine("Attacker use charge move");
                    if (write_battle_log)
                        Battle_logger.LogAttackerAction("Charge move", attacker, defender, timer);

                    defender.incoming_damage = attacker.c_damage;
                    attacker.waiting = attacker.c_cd;
                    attacker.current_en -= attacker.enrg_cost;
                }
                else
                { //q move
                    //console.WriteLine("Attacker use quick move");
                    if (write_battle_log)
                        Battle_logger.LogAttackerAction("Quick move", attacker, defender, timer);

                    defender.incoming_damage = attacker.q_damage;
                    attacker.waiting = attacker.q_cd;
                    attacker.current_en += attacker.enrg_gain;
                    if (attacker.current_en > 100) attacker.current_en = 100;
                }
            }
            else if (defender.waiting < attacker.time_til_damage)
            {   //defender acts
                timer += defender.waiting;
                attacker.waiting -= defender.waiting;
                attacker.time_til_damage -= defender.waiting;

                if (defender.enrg_cost <= defender.current_en &&
                    delayed_charge)
                { //c move
                    //console.WriteLine("Defender use charge move");
                    if (write_battle_log)
                        Battle_logger.LogDefenderAction("Charge move !", attacker, defender, timer);

                    delayed_charge = false;

                    attacker.incoming_damage = defender.c_damage;
                    attacker.time_til_damage = defender.c_cd - defender.end_dodge_c;
                    defender.waiting = defender.c_cd;
                    defender.current_en -= defender.enrg_cost;
                    defender.next_attack_is_charged = true;
                }
                else
                { //q move
                    //console.WriteLine("Defender use quick move");
                    if (write_battle_log)
                        Battle_logger.LogDefenderAction("Quick move", attacker, defender, timer);
                    
                    if (defender.enrg_cost <= defender.current_en)
                        delayed_charge = true; //1 move delay for charged attacks

                    attacker.incoming_damage = defender.q_damage;
                    attacker.time_til_damage = defender.q_cd - defender.end_dodge_q;
                    defender.waiting = defender.q_cd;
                    defender.current_en += defender.enrg_gain;
                    if (defender.current_en > 200) defender.current_en = 200;
                    defender.next_attack_is_charged = false;

                    defender.attacked = true;
                }
            }
            else
            {   //damage reached attacker
                //console.WriteLine("Attacker get hit");
                timer += attacker.time_til_damage;
                attacker.waiting -= attacker.time_til_damage;
                defender.waiting -= attacker.time_til_damage;

                if (write_battle_log && attacker.incoming_damage != 0)
                    Battle_logger.LogIncomingDamage(attacker, defender, timer);

                if (attacker.take_damage())
                    throw new Exception("2"); //defender wins

                attacker.time_til_damage = 100F;
            }
        }

        //input format:
        //pokemon/quick_move/charge_move/level/aIV/dIV/sIV 
        //it's gonna work more efficient if all data would be entered as separate attributes
        //main purpose of using strings in attributes is easy input of my test values
        static void set_battle_options(string defender_string, string attacker_string) {
            string[] df = defender_string.Split('/');
            string[] at = attacker_string.Split('/');

            int defSpecies = 0;
            int atkSpecies = 0;

            //find defender's id
            for (int i = 1; i < pokemonLimit; i++)
                if (pokemonTable.Rows[i][0].ToString() == df[0])
                    defSpecies = i;

            //find attacker's id
            for (int i = 1; i < pokemonLimit; i++)
                if (pokemonTable.Rows[i][0].ToString() == at[0])
                    atkSpecies = i;

            defender.cpm = 
                cp_mult.get_cpm_by_level(Convert.ToSingle(df[3], System.Globalization.CultureInfo.InvariantCulture));            
            defender.attack = Convert.ToInt32(pokemonTable.Rows[defSpecies][3]) + Convert.ToInt32(df[4]);
            defender.defense = Convert.ToInt32(pokemonTable.Rows[defSpecies][4]) + Convert.ToInt32(df[5]);
            defender.hp = (int)Math.Floor((Convert.ToInt32(pokemonTable.Rows[defSpecies][5]) + Convert.ToInt32(df[6]))
                          * defender.cpm)
                          * global_hp_rate; //(base_stamina + sIV) * cpm

            attacker.cpm =
                cp_mult.get_cpm_by_level(Convert.ToSingle(at[3], System.Globalization.CultureInfo.InvariantCulture));
            attacker.attack = Convert.ToInt32(pokemonTable.Rows[atkSpecies][3]) + Convert.ToInt32(at[4]);
            attacker.defense = Convert.ToInt32(pokemonTable.Rows[atkSpecies][4]) + Convert.ToInt32(at[5]);
            attacker.hp = (int)Math.Floor((Convert.ToInt32(pokemonTable.Rows[atkSpecies][5]) + Convert.ToInt32(at[6])) 
                          * attacker.cpm)
                          * global_hp_rate; //(base_stamina + sIV) * cpm
            
            QMove q = qMovesList.Find(x => x.name == at[1]);
            CMove c = cMovesList.Find(x => x.name == at[2]);
            
            q = qMovesList.Find(x => x.name == df[1]);
            c = cMovesList.Find(x => x.name == df[2]);

            defender.q_move = df[1];
            defender.c_move = df[2];

            defender.q_power = q.power;
            defender.q_cd = q.cd + 2;
            defender.enrg_gain = q.energy_gain;

            defender.c_power = c.power;
            defender.c_cd = c.cd + 2;

            defender.start_dodge_q = defender.q_cd - q.dmg_start - user_reaction_constant;
            defender.end_dodge_q = defender.start_dodge_q - 0.7F + user_reaction_constant;
            defender.start_dodge_c = defender.c_cd - c.dmg_start - user_reaction_constant;
            defender.end_dodge_c = defender.start_dodge_c - 0.7F + user_reaction_constant;
            defender.enrg_cost = c.energy_cost;

            string defence_type = pokemonTable.Rows[atkSpecies][1].ToString().Trim();
            bool type_found = false;
            for (int i = 1; i < typeLimit; i++)
                if (typeTable.Rows[i][0].ToString() == defence_type)
                {
                    type_found = true;
                    //Type effectiveness
                    defender.q_mult = NumToDamageRatio(typeTable.Rows[i][q.type].ToString().Trim()) / 10000F;
                    defender.c_mult = NumToDamageRatio(typeTable.Rows[i][c.type].ToString().Trim()) / 10000F;
                    //STAB
                    string[] hitter_type = pokemonTable.Rows[defSpecies][1].ToString().Trim().Split('/');
                    foreach (string d in hitter_type)
                        if (q.type == d) defender.q_mult *= 1.25F;
                    foreach (string d in hitter_type)
                        if (c.type == d) defender.c_mult *= 1.25F;
                    break;
                }

            if (!type_found)
                throw new Exception("Type " + defence_type + " of " + attacker.pokemonName + " is not found");

            attacker.q_move = at[1];
            attacker.c_move = at[2];

            attacker.q_power = q.power;
            attacker.q_cd = q.cd;
            attacker.enrg_gain = q.energy_gain;

            attacker.c_power = c.power;
            attacker.c_cd = c.cd;
            attacker.enrg_cost = c.energy_cost;

            defence_type = pokemonTable.Rows[defSpecies][1].ToString().Trim();
            type_found = false;
            for (int i = 1; i < typeLimit; i++)
                if (typeTable.Rows[i][0].ToString() == defence_type)
                {
                    type_found = true;
                    //Type effectiveness
                    attacker.q_mult = NumToDamageRatio(typeTable.Rows[i][q.type].ToString().Trim()) / 10000F;
                    attacker.c_mult = NumToDamageRatio(typeTable.Rows[i][c.type].ToString().Trim()) / 10000F;
                    //STAB
                    string[] hitter_type = pokemonTable.Rows[atkSpecies][1].ToString().Trim().Split('/');
                    foreach (string a in hitter_type)
                        if (q.type == a) attacker.q_mult *= 1.25F;
                    foreach (string a in hitter_type)
                        if (c.type == a) attacker.c_mult *= 1.25F;
                    break;
                }

            if (!type_found)
                throw new Exception("Type " + defence_type + " of " + defender.pokemonName + " is not found");

            defender.q_damage = 
                1 + (int)Math.Floor(0.5 * (float)defender.q_power * defender.q_mult
                                        * (float)defender.attack  * defender.cpm
                                        / (float)attacker.defense / attacker.cpm);

            defender.c_damage = 
                1 + (int)Math.Floor(0.5 * (float)defender.c_power * defender.c_mult
                                        * (float)defender.attack  * defender.cpm
                                        / (float)attacker.defense / attacker.cpm);
             
            attacker.q_damage = 
                1 + (int)Math.Floor(0.5 * (float)attacker.q_power * attacker.q_mult
                                        * (float)attacker.attack  * attacker.cpm
                                        / (float)defender.defense / defender.cpm);

            attacker.c_damage = 
                1 + (int)Math.Floor(0.5 * (float)attacker.c_power * attacker.c_mult
                                        * (float)attacker.attack  * attacker.cpm
                                        / (float)defender.defense / defender.cpm);

            attacker.use_charges = true;
            if ((float)attacker.q_power * attacker.q_mult / attacker.q_cd >
                (float)attacker.c_power * attacker.c_mult / attacker.c_cd)
            {
                attacker.enrg_cost = 900; //this will not let attacker to use charges
                attacker.c_move = "Dont use charge";
                attacker.use_charges = false;
            }

            //to find situation when defender have low hp 
            //and it's better to use quick move then charged
            defender.overkill_hp_point = 
                attacker.q_damage * (int)Math.Floor(attacker.c_cd / attacker.q_cd);
        }
    }
}
