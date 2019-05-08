using System.Collections.Generic;

namespace GpxPathViewer
{
    public class Hub
    {
        public string Name { get; }
        public double Lat { get; }
        public double Lon { get; }

        public Hub(string name, double lat, double lon)
        {
            Name = name;
            Lat = lat;
            Lon = lon;
        }
        public bool IsInside(Node node)
        {
            return Lat - 0.001 <= node.Lat && node.Lat <= Lat + 0.001
                                           && Lon - 0.001 <= node.Lon && node.Lon <= Lon + 0.001;
        }

        public static IEnumerable<Hub> GetWaveloHubs()
        {
            yield return new Hub("001 Plac Wszystkich Świętych", 50.05909239149709, 19.937463104724884);
            yield return new Hub("002 Teatr Bagatela I", 50.06323613499392, 19.933394193649292);
            yield return new Hub("003 Cracovia Błonia", 50.05918279483235, 19.923405647277832);
            yield return new Hub("004 Plac Inwalidów", 50.06953414613764, 19.925651997327805);
            yield return new Hub("005 Politechnika", 50.07064800580462, 19.943673759698868);
            yield return new Hub("006 Dworzec Główny Wschód", 50.06877835965966, 19.94964972138405);
            yield return new Hub("007 Rondo Mogilskie", 50.06574992917475, 19.95924800634384);
            yield return new Hub("008 Rondo Grzegórzeckie I", 50.058261532682295, 19.96016800403595);
            yield return new Hub("009 Miodowa", 50.05356885259019, 19.947848618030548);
            yield return new Hub("010 Plac Bohaterów Getta", 50.04661681418889, 19.955340027809143);
            yield return new Hub("011 Korona", 50.04365760810532, 19.94696080684662);
            yield return new Hub("012 Plac Wolnica", 50.04844082062142, 19.943752884864807);
            yield return new Hub("013 Poczta Główna", 50.0593394936605, 19.942529797554016);
            yield return new Hub("014 Wawel", 50.053982171363, 19.938734471797943);
            yield return new Hub("015 Centrum Kongresowe ICE", 50.0487353420475, 19.93106335401535);
            yield return new Hub("016 Szwedzka", 50.04678044343913, 19.924754798412323);
            yield return new Hub("017 Jubilat", 50.05565951979328, 19.927643537521362);
            yield return new Hub("018 Salwator", 50.05317240730694, 19.91706222295761);
            yield return new Hub("019 Cichy Kącik", 50.062708393500046, 19.902658760547638);
            yield return new Hub("020 Uniwersytet Pedagogiczny", 50.073633090815754, 19.90987926721573);
            yield return new Hub("021 Łobzów PKP", 50.08172218453383, 19.915711730718613);
            yield return new Hub("022 Nowy Kleparz", 50.0736876605636, 19.935908764600754);
            yield return new Hub("023 Bratysławska", 50.08447217145953, 19.933581948280334);
            yield return new Hub("024 Krowodrza Górka", 50.08837916250239, 19.93196189403534);
            yield return new Hub("025 Mackiewicza", 50.09020226929019, 19.94391918182373);
            yield return new Hub("026 Uniwersytet Rolniczy", 50.08256623294734, 19.952228665351868);
            yield return new Hub("027 Wieczysta", 50.070985084461846, 19.984643161296844);
            yield return new Hub("028 Rondo Kocmyrzowskie", 50.080425101453486, 20.02680480480194);
            yield return new Hub("029 DH Wanda", 50.08428285492882, 20.019707679748535);
            yield return new Hub("030 Rondo Czyżyńskie ", 50.074440442770054, 20.018940567970276);
            yield return new Hub("031 Azory", 50.086406432220656, 19.90881711244583);
            yield return new Hub("032 Miasteczko Studenckie AGH", 50.06965121427996, 19.90564674139023);
            yield return new Hub("033 Starowiślna", 50.057189403076976, 19.945429265499115);
            yield return new Hub("034 Stradom I", 50.052146319497425, 19.942036271095276);
            yield return new Hub("035 Most Grunwaldzki", 50.04986087905614, 19.935845732688904);
            yield return new Hub("036 Kościuszki", 50.05475242696107, 19.926881790161133);
            yield return new Hub("037 Reymonta", 50.06558051037513, 19.914135932922363);
            yield return new Hub("038 Czarnowiejska", 50.06672166466257, 19.921651482582092);
            yield return new Hub("039 Kawiory", 50.06867196209764, 19.913619607686996);
            yield return new Hub("040 AGH Czysta", 50.06376473140594, 19.924612641334534);
            yield return new Hub("041 Rondo Hipokratesa", 50.0899513686653, 20.024286210536957);
            yield return new Hub("042 Plac Bieńczycki", 50.082373810307914, 20.031890273094177);
            yield return new Hub("043 Dunikowskiego", 50.09067584615839, 20.01686990261078);
            yield return new Hub("044 Wiślicka", 50.08807318178654, 20.000991225242615);
            yield return new Hub("045 Rondo Grzegórzeckie II", 50.05775440145301, 19.957590401172638);
            yield return new Hub("046 Fabryka Schindlera", 50.0473703663003, 19.961487650871277);
            yield return new Hub("047 Bronowice Małe", 50.081967621106756, 19.882898926734924);
            yield return new Hub("048 Francesco Nullo", 50.0604105417851, 19.967544078826904);
            yield return new Hub("049 Ofiar Dąbia", 50.059876742465846, 19.976245164871216);
            yield return new Hub("050 Osiedle Zgody - UMK", 50.07702525696512, 20.031874179840088);
            yield return new Hub("051 Hala Targowa", 50.05884563235702, 19.94859293103218);
            yield return new Hub("052 Balicka Wiadukt", 50.081447830868754, 19.888695180416107);
            yield return new Hub("053 Bronowice", 50.0782248354428, 19.899955093860626);
            yield return new Hub("054 Prokocim Rynek", 50.02296304444933, 19.9984310567379);
            yield return new Hub("055 Smoleńsk", 50.05921275703426, 19.931900203227997);
            yield return new Hub("056 Garbarska", 50.065785224573574, 19.935499727725983);
            yield return new Hub("057 Powstania Warszawskiego - UMK", 50.06190342518617, 19.96073395013809);
            yield return new Hub("058 Grunwaldzka - UMK", 50.06731116945346, 19.9653097987175);
            yield return new Hub("059 Rondo Dywizjonu 308", 50.0672064916558, 20.005213022232056);
            yield return new Hub("060 Osiedle Kolorowe", 50.07253651528775, 20.028693079948425);
            yield return new Hub("061 Galicyjska", 50.06361080189364, 20.00738024711609);
            yield return new Hub("062 Plac Centralny", 50.07276375097176, 20.03770262002945);
            yield return new Hub("063 Zdrowa", 50.08856398870202, 19.938827008008957);
            yield return new Hub("064 Stradom II", 50.05121596710114, 19.94093656539917);
            yield return new Hub("065 Reymonta - Kapitol", 50.06676539556376, 19.90673303604126);
            yield return new Hub("066 Uniwersytet Ekonomiczny", 50.06804494218963, 19.9531888961792);
            yield return new Hub("067 Cmentarz Rakowicki", 50.07404348227114, 19.957863986492157);
            yield return new Hub("068 Miechowity", 50.08321646989565, 19.97129648923874);
            yield return new Hub("069 Rondo Młyńskie", 50.08126442254536, 19.973715841770172);
            yield return new Hub("070 Narzymskiego I", 50.07288356589298, 19.96611177921295);
            yield return new Hub("071 Biprostal", 50.0731627884594, 19.915527999401093);
            yield return new Hub("072 Urzędnicza", 50.07029164241049, 19.919108748435974);
            yield return new Hub("073 Łobzowska", 50.06756184507964, 19.933899119496346);
            yield return new Hub("074 Struga", 50.074934487791666, 20.046175718307495);
            yield return new Hub("075 Zalew Nowohucki", 50.07994625783793, 20.050123929977417);
            yield return new Hub("076 Żeromskiego", 50.078713505276326, 20.04009246826172);
            yield return new Hub("077 Osiedle Na Skarpie", 50.06973557228277, 20.045118927955627);
            yield return new Hub("078 Okulickiego", 50.08708604599465, 19.994824826717377);
            yield return new Hub("079 Osiedle Piastów", 50.10175847403336, 20.012680292129517);
            yield return new Hub("080 Teatr Bagatela II", 50.06363559602367, 19.932643175125122);
            yield return new Hub("081 Krowoderska", 50.06896257291082, 19.936737567186356);
            yield return new Hub("082 Racławicka", 50.078949317504865, 19.92094874382019);
            yield return new Hub("083 Mazowiecka", 50.078501099838185, 19.918212890625);
            yield return new Hub("084 Wesele", 50.07923780022075, 19.897278249263763);
            yield return new Hub("085 Prokocim Szpital", 50.01438816371781, 19.999006390571594);
            yield return new Hub("086 Wrocławska", 50.07615666824961, 19.928609132766724);
            yield return new Hub("087 Cmentarz Podgórski - UMK", 50.03803409700538, 19.965883791446686);
            yield return new Hub("088 Kraków Płaszów", 50.03437057964414, 19.973934441804886);
            yield return new Hub("089 Dauna", 50.01871500298009, 19.977843761444092);
            yield return new Hub("090 Wlotowa", 50.02003684775457, 19.98773843050003);
            yield return new Hub("091 Myśliwska", 50.0449822111873, 20.00064253807068);
            yield return new Hub("092 Bieżanowska", 50.023910832203256, 19.979369938373566);
            yield return new Hub("093 Kuklińskiego", 50.04443876589035, 19.97318208217621);
            yield return new Hub("094 Rzebika", 50.040774876434824, 19.9920192360878);
            yield return new Hub("095 Saska", 50.045187616214434, 19.98346835374832);
            yield return new Hub("096 Św. Wawrzyńca", 50.05024667297083, 19.952298402786255);
            yield return new Hub("097 Ludwinów", 50.043664153534905, 19.934633374214172);
            yield return new Hub("098 Rondo Matecznego I", 50.037133102696856, 19.9405637383461);
            yield return new Hub("099 Nowosądecka", 50.014437285024655, 19.965956211090088);
            yield return new Hub("100 Kurdwanów", 50.01388229214766, 19.950855374336243);
            yield return new Hub("101 Wysłouchów", 50.008030682891075, 19.9519282579422);
            yield return new Hub("102 Czerwone Maki", 50.017443102660366, 19.890020191669464);
            yield return new Hub("103 Grota-Roweckiego", 50.03196282485682, 19.917397499084473);
            yield return new Hub("104 Norymberska", 50.02872013449644, 19.911528825759888);
            yield return new Hub("105 Borsucza", 50.030649924006696, 19.928861260414124);
            yield return new Hub("106 Kobierzyńska", 50.034722901822654, 19.926136136054993);
            yield return new Hub("107 Miłkowskiego", 50.028801980264845, 19.917773008346558);
            yield return new Hub("108 Stojałowskiego", 50.00541732670436, 19.954465627670288);
            yield return new Hub("109 Kampus UJ - Łojasiewicza", 50.02988387579829, 19.906998574733734);
            yield return new Hub("110 Zachodnia", 50.02414932689045, 19.910989701747894);
            yield return new Hub("111 Ruczaj", 50.026726512565496, 19.906282424926758);
            yield return new Hub("112 Chmieleniec", 50.020193329582284, 19.898050725460052);
            yield return new Hub("113 Rynek Dębnicki", 50.05184363886395, 19.92653578519821);
            yield return new Hub("114 Zielińskiego", 50.04641124299888, 19.910563230514526);
            yield return new Hub("115 Zalew Bagry", 50.033921083897354, 19.994674623012543);
            yield return new Hub("116 Turniejowa", 50.00822443793693, 19.959771037101746);
            yield return new Hub("117 Narzymskiego II", 50.07372157277351, 19.966047406196594);
            yield return new Hub("118 Olszecka", 50.08835076744962, 19.967411309480667);
            yield return new Hub("119 Osiedle Złotego Wieku", 50.093498623925754, 20.00488579273224);
            yield return new Hub("120 Kleeberga", 50.09527714331474, 20.012422800064087);
            yield return new Hub("121 Politechnika - Wydział Mechaniczny", 50.07362964791814, 19.99508500099182);
            yield return new Hub("122 Łokietka", 50.08536539398705, 19.923413693904877);
            yield return new Hub("123 Rondo Barei", 50.089453864684614, 19.976235777139664);
            yield return new Hub("124 Bociana", 50.09430168452013, 19.951499104499817);
            yield return new Hub("125 Pachońskiego", 50.09509819282593, 19.9316668510437);
            yield return new Hub("126 Mistrzejowice", 50.09502833347759, 19.996764063835144);
            yield return new Hub("127 Prądnik Czerwony", 50.096881470571, 19.973951876163483);
            yield return new Hub("128 Mały Płaszów", 50.04020054817647, 20.00033676624298);
            yield return new Hub("129 Bohomolca", 50.095220360827824, 19.988728165626526);
            yield return new Hub("130 Akacjowa", 50.08451743627396, 19.980056583881378);
            yield return new Hub("131 Radzikowskiego", 50.08370250841048, 19.904402196407318);
            yield return new Hub("132 Dworcowa", 50.03283634994649, 19.971084594726562);
            yield return new Hub("133 Łagiewniki", 50.02969106949063, 19.936422407627106);
            yield return new Hub("134 Borek Fałęcki", 50.01179583772476, 19.927258640527725);
            yield return new Hub("135 Zabłocie", 50.05011405737106, 19.962252080440518);
            yield return new Hub("136 Rondo Matecznego II", 50.03641022766885, 19.94003400206566);
            yield return new Hub("137 Makowskiego", 50.08769319929496, 19.917023330926895);
            yield return new Hub("138 Pawia", 50.06542452077584, 19.9451744556427);
            yield return new Hub("139 Teligi", 50.016498634413246, 20.008791089057922);
            yield return new Hub("140 Ćwiklińskiej ", 50.01625251805616, 20.01962184906006);
            yield return new Hub("141 Macedońska", 50.02229958208405, 19.97061923146248);
            yield return new Hub("142 Powiśle", 50.0550622939907, 19.932632446289062);
            yield return new Hub("143 Sławkowska", 50.06606500504467, 19.939300417900085);
            yield return new Hub("144 Kopernika", 50.06117286001188, 19.94447574019432);
            yield return new Hub("145 Ugorek", 50.07550082629386, 19.979895651340485);
            yield return new Hub("146 Brogi-Bosaków", 50.08311716295388, 19.96382385492325);
            yield return new Hub("147 Prądnicka", 50.08060135200847, 19.938082695007324);
            yield return new Hub("148 Centralna", 50.06891512501757, 20.01227393746376);
            yield return new Hub("149 AWF", 50.07358144732596, 19.999494552612305);
            yield return new Hub("150 Kampus UJ - Gronostajowa", 50.027899606104775, 19.904812574386597);
            yield return new Hub("151 IKEA", 50.089323249727315, 19.898375272750854);
            yield return new Hub("152 Rondo Czyżyńskie II", 50.0741098141582, 20.01631200313568);
            yield return new Hub("153 Medweckiego", 50.07632467229803, 20.008158087730408);
            yield return new Hub("154 Włodarczyka", 50.08315611659137, 19.99988079071045);
            yield return new Hub("155 Marii Dąbrowskiej", 50.08194949151816, 20.014702677726742);
            yield return new Hub("156 Galeria Bronowice", 50.09318900508692, 19.897256791591644);
            yield return new Hub("157 Quattro Business Park", 50.08642909179735, 19.975504875183105);
            yield return new Hub("158 Bonarka Shopping Center", 50.02883175077807, 19.95389834046364);
            yield return new Hub("159 Bulwary Kurlandzkie", 50.05306262784181, 19.959607422351837);
            yield return new Hub("160 B4B bonarka for business", 50.026619678257035, 19.951930940151215);
            yield return new Hub("161 Centrum Biurowe Jasnogórska 1", 50.08903164478005, 19.89177569746971);
            yield return new Hub("162 Centrum Serenada", 50.08862387604878, 19.987015575170517);
            yield return new Hub("163 Fitness Platinium Bratysławska", 50.085202584548114, 19.936395585536957);
            yield return new Hub("164 Fitness Platinium Aleja Pokoju", 50.06078591978109, 19.96878057718277);
            yield return new Hub("165 Kapelanka 42", 50.035617051203864, 19.925450831651688);
            yield return new Hub("166 Dot Office", 50.02199330389444, 19.89051640033722);
            yield return new Hub("167 ABB Business Services", 50.06451161142871, 19.96013581752777);
            yield return new Hub("Hot Spot Plac Szczepański", 50.063796755161334, 19.935444742441177);
            yield return new Hub("Hot Spot UJ Collegium Medicum", 50.011369228082216, 19.993320107460022);
            yield return new Hub("Hot Spot UJ Ruczaj", 50.02682197308468, 19.90194797515869);
            yield return new Hub("Pawia II", 50.06602416795832, 19.945635460317135);
            yield return new Hub("Stacja Mobilna M01", 50.065131823504665, 19.926462024450302);


        }
    }
}