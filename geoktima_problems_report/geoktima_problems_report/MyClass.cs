using Mono.Security.X509;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace geoktima_problems_report
{
    class MyClass
    {
        public String reportDir = "";
        public String currentDate = "";
        String User_Id = "vijfkdj", Password = "sdsds";
        public String currentDataBase = "";
        String connstring = "";
        public List<String> databaseList = new List<String>() { "gkt001", "gkt002", "gkt003", "gkt004", "gkt005", "gkt007" };
        public String errorLogsString = "", reportLogsString = "";
        public void databaseReadAndWriteToCsvs()
        {
            try
            {
                DataTable aitiseis_provlimataSaromenonDt = aitiseis_provlimataSaromenon("0");
                writeTocsv(aitiseis_provlimataSaromenonDt, "Αιτήσεις_Προβλήματα_σαρωμένων_εγγράφων_Προβληματική_σάρωση.csv");
                DataTable aitiseis_provlimataSaromenonDt1 = aitiseis_provlimataSaromenon("1");
                writeTocsv(aitiseis_provlimataSaromenonDt1, "Αιτήσεις_Προβλήματα_σαρωμένων_εγγράφων_Ασάρωτο_έγγραφο.csv");
                DataTable aitiseis_paratiriseis_sarosisDt = aitiseis_paratiriseis_sarosis("problem_descr");
                writeTocsv(aitiseis_paratiriseis_sarosisDt, "Αιτήσεις_Περιλήψεις_Αιτήσεις_Παρατηρήσεις_σάρωσης_περιέχει_ΣΑΡΩ.csv");
                DataTable aitiseis_sxoliaDt = aitiseis_paratiriseis_sarosis("comments");
                writeTocsv(aitiseis_sxoliaDt, "Αιτήσεις_Περιλήψεις_Αιτήσεις_Σχόλια_περιέχει_ΣΑΡΩ.csv");
                DataTable perilipseis_provlim_saro_eggraf_provlimatiki_saroDt = perilipseis_provlim_saro_eggraf_provlimatiki_saro("1");
                writeTocsv(perilipseis_provlim_saro_eggraf_provlimatiki_saroDt, "Περιλήψεις_Προβλήματα_σαρωμένων_εγγράφων__Προβληματική_σάρωση.csv");
                DataTable perilipseis_provlim_saro_eggraf_asarotoDt = perilipseis_provlim_saro_eggraf_provlimatiki_saro("2");
                writeTocsv(perilipseis_provlim_saro_eggraf_asarotoDt, "Περιλήψεις_Προβλήματα_σαρωμένων_εγγράφων_Ασάρωτο_έγγραφο.csv");
                DataTable perilipseis_paratiriseis_sarosis_saroDt = perilipseis_paratiriseis_sarosis_saro("problem_descr");
                writeTocsv(perilipseis_paratiriseis_sarosis_saroDt, "Περιλήψεις_Παρατηρήσεις_σάρωσης_ΣΑΡΩ.csv");
                DataTable perilipseis_sxolia_saroDt = perilipseis_paratiriseis_sarosis_saro("s.comments");
                writeTocsv(perilipseis_sxolia_saroDt, "Περιλήψεις_σχόλια_ΣΑΡΩ.csv");
                DataTable lathi_provlimata_perigrafi_provlimatos_saroDt = lathi_provlimata_perigrafi_provlimatos_saro("i.description");
                writeTocsv(lathi_provlimata_perigrafi_provlimatos_saroDt, "Λάθη_Προβλήματα_Περιγραφή_προβλήματος_ΣΑΡΩ.csv");
                DataTable lathi_provlimata_sxolia_diorthosis_saroDt = lathi_provlimata_perigrafi_provlimatos_saro("check_description ");
                writeTocsv(lathi_provlimata_sxolia_diorthosis_saroDt, "Λάθη_Προβλήματα_Σχόλια_Διόρθωσης_ΣΑΡΩ.csv");
            }
            catch (Exception ex) 
            {
                errorLogsString = errorLogsString + " (function: databaseReadAndWriteToCsvs) " + ex.Message + "\n";
            }
        }
        private DataTable aitiseis_provlimataSaromenon(String problem_code)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                connstring = "User Id=" + User_Id + ";Password=" + Password + ";Server=192.168.26.34;Port=5432;Database=" + currentDataBase + ";Pooling=false;Preload Reader=true;CommandTimeout=10000";
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = @"select appl_id, g_eis_id, g_delivery_id, appl_sub_date, appl_pro_date, entry_date,
                g_prop_id, a.app_type_code, anadoxos_status,
                a.is_deleted, rec_modif_code, status_code, app_type_descr, comments, appl_status_descr, arxeiothetisi, problem_code_descr,
                case
                when coalesce(i.file_type, -999) = 2 then i.problem_descr
                else ''
                end problem_descr,
                case
                when coalesce(i.file_type, -999) = 2 then i.problem_code
                else -1
                end problem_code,
                coalesce(status_code,1) st_code
                from ktinc.sup_t_application a
                LEFT JOIN public.ktscanned_issues i ON i.file_id = a.appl_id
                LEFT JOIN ktinc.appl_status_lut on a.status_code = ktinc.appl_status_lut.appl_status_code
                LEFT JOIN ktinc.sup_l_app_type ON a.app_type_code = ktinc.sup_l_app_type.app_type_code
                LEFT JOIN ktinc.scan_issues_lut ON i.problem_code = ktinc.scan_issues_lut.problem_code
                where i.problem_code = " + problem_code + "order by appl_id";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                da.Fill(ds);
                dt = ds.Tables[0];
                dt.Columns["appl_id"].ColumnName = "APPL_ID";
                dt.Columns["g_eis_id"].ColumnName = "G_EIS_ID";
                dt.Columns["g_delivery_id"].ColumnName = "G_DELIVERY_ID";
                dt.Columns["appl_sub_date"].ColumnName = "Ημ/νία υποβολής";
                dt.Columns["appl_pro_date"].ColumnName = "Ημ/νία οριστικής καταχώρισης";
                dt.Columns["entry_date"].ColumnName = "Ημ/νία εισαιγωγής Συστήματος";
                dt.Columns["g_prop_id"].ColumnName = "G_PROP_ID";
                dt.Columns["app_type_descr"].ColumnName = "Είδος Αίτησης";
                dt.Columns["g_delivery_id"].ColumnName = "G_DELIVERY_ID";
                dt.Columns["arxeiothetisi"].ColumnName = "Φάκελος αρχειοθέτησης";
                dt.Columns["comments"].ColumnName = "Σχόλια";
                dt.Columns["problem_descr"].ColumnName = "Παρατηρήσεις Σάρωσης";
                dt.Columns["problem_code_descr"].ColumnName = "Προβλήματα Σαρωμένων Εγγράφων";
                dt.Columns["appl_status_descr"].ColumnName = "Κατάσταση Επεξεργασίας Αίτησης";
                dt.Columns.Remove("is_deleted");
                dt.Columns.Remove("rec_modif_code");
                dt.Columns.Remove("status_code");
                dt.Columns.Remove("anadoxos_status");
                dt.Columns.Remove("app_type_code");
                dt.Columns.Remove("problem_code");
                dt.Columns.Remove("st_code");
                conn.Close();
                reportLogsString = reportLogsString + " Σωστή ανάγνωση από την βάση δεδομένων: " + currentDataBase + " για της aitiseis_provlimataSaromenon.\n";
            }
            catch (Exception ex)
            {
                errorLogsString = errorLogsString + " (function: aitiseis_provlimataSaromenon) " + ex.Message +"\n";
            }
            return dt;
        }
        private DataTable aitiseis_paratiriseis_sarosis(String selectFieldName)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                connstring = "User Id=" + User_Id + ";Password=" + Password + ";Server=192.168.24.27;Port=5432;Database=" + currentDataBase + ";Pooling=false;Preload Reader=true;CommandTimeout=10000";
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = @"select appl_id, appl_sub_date, appl_pro_date, entry_date, g_eis_id,
                g_prop_id, g_ben_id, a.app_type_code, anadoxos_status, arxeiothetisi,
                a.is_deleted, rec_modif_code, g_delivery_id, status_code, app_type_descr, comments, problem_code_descr, appl_status_descr,
                case
                when coalesce(i.file_type, -999) = 2 then i.problem_descr
                else ''
                end problem_descr,
                case
                when coalesce(i.file_type, -999) = 2 then i.problem_code
                else -1
                end problem_code,
                coalesce(status_code,1) st_code
                from ktinc.sup_t_application a
                LEFT JOIN public.ktscanned_issues i ON i.file_id = a.appl_id
                LEFT JOIN ktinc.appl_status_lut on a.status_code = ktinc.appl_status_lut.appl_status_code
                LEFT JOIN ktinc.sup_l_app_type ON a.app_type_code = ktinc.sup_l_app_type.app_type_code
                LEFT JOIN ktinc.scan_issues_lut ON i.problem_code = ktinc.scan_issues_lut.problem_code
                where " + selectFieldName + " like '%ΣΑΡΩ%' OR " + selectFieldName + " like '%σαρω%' OR " + selectFieldName + " like '%σαρώ%' order by appl_id";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                da.Fill(ds);
                dt = ds.Tables[0];
                dt.Columns["appl_id"].ColumnName = "APPL_ID";
                dt.Columns["g_eis_id"].ColumnName = "G_EIS_ID";
                dt.Columns["g_delivery_id"].ColumnName = "G_DELIVERY_ID";
                dt.Columns["appl_sub_date"].ColumnName = "Ημ/νία υποβολής";
                dt.Columns["appl_pro_date"].ColumnName = "Ημ/νία οριστικής καταχώρισης";
                dt.Columns["entry_date"].ColumnName = "Ημ/νία εισαιγωγής Συστήματος";
                dt.Columns["g_prop_id"].ColumnName = "G_PROP_ID";
                dt.Columns["app_type_descr"].ColumnName = "Είδος Αίτησης";
                dt.Columns["g_delivery_id"].ColumnName = "G_DELIVERY_ID";
                dt.Columns["arxeiothetisi"].ColumnName = "Φάκελος αρχειοθέτησης";
                dt.Columns["comments"].ColumnName = "Σχόλια";
                dt.Columns["problem_descr"].ColumnName = "Παρατηρήσεις Σάρωσης";
                dt.Columns["problem_code_descr"].ColumnName = "Προβλήματα Σαρωμένων Εγγράφων";
                dt.Columns["appl_status_descr"].ColumnName = "Κατάσταση Επεξεργασίας Αίτησης";
                dt.Columns.Remove("is_deleted");
                dt.Columns.Remove("rec_modif_code");
                dt.Columns.Remove("status_code");
                dt.Columns.Remove("anadoxos_status");
                dt.Columns.Remove("app_type_code");
                dt.Columns.Remove("problem_code");
                dt.Columns.Remove("st_code");
                conn.Close();
                reportLogsString = reportLogsString + " Σωστή ανάγνωση από την βάση δεδομένων: " + currentDataBase + " για της aitiseis_paratiriseis_sarosis.\n";
            }
            catch (Exception ex)
            {
                errorLogsString = errorLogsString + " (function: aitiseis_paratiriseis_sarosis) " + ex.Message + "\n";
            }
            return dt;
        }
        private DataTable perilipseis_provlim_saro_eggraf_provlimatiki_saro(String fieldValue)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                connstring = "User Id=" + User_Id + ";Password=" + Password + ";Server=192.168.24.27;Port=5432;Database=" + currentDataBase + ";Pooling=false;Preload Reader=true;CommandTimeout=10000";
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = @"select s.summary_id,s.cover_id,c.cover_date,doc_type_descr, sum_doc_num,sum_doc_date,s.sum_issuer_type, trim(coalesce(surname,'') || ' ' || coalesce(name, '') || coalesce(legal_name,'') ||' (' || coalesce(issuer_subcat_descr, '') || '-' || coalesce(seat, '') || ')' || '-'||di.g_issuer_id) as issuer_name,
                s.type,s.g_delivery_id,sum_status_descr,s.comments,coalesce(s.g_doc_id,'-1') as g_doc_id,s.g_doc_id gdocid, sum_doc_type, problem_code_descr,
                case
                when coalesce(i.file_type, -999) = 3 then i.problem_descr
                else ''
                end problem_descr,
                case
                when coalesce(i.file_type, -999) = 3 then i.problem_code
                else -1
                end problem_code
                from ktinc.summary s JOIN ktinc.covering c on c.cover_id = s.cover_id
                LEFT JOIN ktinc.doc d on s.g_doc_id = d.g_doc_id
                LEFT JOIN ktinc.doc_issuer di on di.g_issuer_id = s.sum_issuer_type
                LEFT JOIN public.ktscanned_issues i ON i.file_id = s.summary_id
                LEFT JOIN ktinc.issuer_type_lut t on di.issuer_type = t.issuer_type
                LEFT JOIN ktinc.doc_type_lut ON s.sum_doc_type = ktinc.doc_type_lut.doc_type
                LEFT JOIN ktinc.scan_issues_lut ON i.problem_code = ktinc.scan_issues_lut.problem_code
            LEFT JOIN ktinc.summary_status_lut ON s.status_code = ktinc.summary_status_lut.sum_status_code
                where i.problem_code = " + fieldValue + " order by s.summary_id";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                da.Fill(ds);
                dt = ds.Tables[0];
                dt.Columns["summary_id"].ColumnName = "SUMMARY_ID";
                dt.Columns["cover_id"].ColumnName = "COVER_ID";
                dt.Columns["cover_date"].ColumnName = "Ημ/νία Διαβιβαστικού";
                dt.Columns["sum_doc_num"].ColumnName = "Αριθμός Τίτλου";
                dt.Columns["sum_doc_date"].ColumnName = "Ημ/νία Τίτλου";
                dt.Columns["comments"].ColumnName = "Σχόλια";
                dt.Columns["gdocid"].ColumnName = "G_DOC_ID";
                dt.Columns["problem_descr"].ColumnName = "Παρατηρήσεις Σάρωσης";
                dt.Columns["issuer_name"].ColumnName = "Εκδότης";
                dt.Columns["doc_type_descr"].ColumnName = "Είδος τίτλου";
                dt.Columns["problem_code_descr"].ColumnName = "Προβλήματα Σαρωμένων Εγγράφων";
                dt.Columns["sum_status_descr"].ColumnName = "Κατάσταση Επεξεργασίας";
                dt.Columns["g_delivery_id"].ColumnName = "G_DELIVERY_ID";
                dt.Columns.Remove("g_doc_id");
                dt.Columns.Remove("sum_issuer_type");
                dt.Columns.Remove("sum_doc_type");
                dt.Columns.Remove("problem_code");
                dt.Columns.Remove("type");
                conn.Close();
                reportLogsString = reportLogsString + " Σωστή ανάγνωση από την βάση δεδομένων: " + currentDataBase + " για της perilipseis_provlim_saro_eggraf_provlimatiki_saro.\n";
            }
            catch (Exception ex)
            {
                errorLogsString = errorLogsString + " (function: perilipseis_provlim_saro_eggraf_provlimatiki_saro) " + ex.Message + "\n";
            }
            return dt;
        }
        private DataTable perilipseis_paratiriseis_sarosis_saro(String fieldName)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                connstring = "User Id=" + User_Id + ";Password=" + Password + ";Server=192.168.25.27;Port=5432;Database=" + currentDataBase + ";Pooling=false;Preload Reader=true;CommandTimeout=10000";
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = @"select s.summary_id,s.cover_id,c.cover_date,doc_type_descr, sum_doc_num,sum_doc_date,s.sum_issuer_type, trim(coalesce(surname,'') || ' ' || coalesce(name, '') || coalesce(legal_name,'') ||' (' || coalesce(issuer_subcat_descr, '') || '-' || coalesce(seat, '') || ')' || '-'||di.g_issuer_id) as issuer_name,
                s.type,s.g_delivery_id,sum_status_descr,s.comments,coalesce(s.g_doc_id,'-1') as g_doc_id,s.g_doc_id gdocid, sum_doc_type, problem_code_descr,
                case
                when coalesce(i.file_type, -999) = 3 then i.problem_descr
                else ''
                end problem_descr,
                case
                when coalesce(i.file_type, -999) = 3 then i.problem_code
                else -1
                end problem_code
                from ktinc.summary s JOIN ktinc.covering c on c.cover_id = s.cover_id
                LEFT JOIN ktinc.doc d on s.g_doc_id = d.g_doc_id
                LEFT JOIN ktinc.doc_issuer di on di.g_issuer_id = s.sum_issuer_type
                LEFT JOIN public.ktscanned_issues i ON i.file_id = s.summary_id
                LEFT JOIN ktinc.issuer_type_lut t on di.issuer_type = t.issuer_type
                LEFT JOIN ktinc.doc_type_lut ON s.sum_doc_type = ktinc.doc_type_lut.doc_type
                LEFT JOIN ktinc.scan_issues_lut ON i.problem_code = ktinc.scan_issues_lut.problem_code
            LEFT JOIN ktinc.summary_status_lut ON s.status_code = ktinc.summary_status_lut.sum_status_code
                where " + fieldName + " like '%ΣΑΡΩ%' or " + fieldName + " like '%σαρω%' or " + fieldName + " like '%σαρώ%' order by s.summary_id";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                da.Fill(ds);
                dt = ds.Tables[0];
                dt.Columns["summary_id"].ColumnName = "SUMMARY_ID";
                dt.Columns["cover_id"].ColumnName = "COVER_ID";
                dt.Columns["cover_date"].ColumnName = "Ημ/νία Διαβιβαστικού";
                dt.Columns["sum_doc_num"].ColumnName = "Αριθμός Τίτλου";
                dt.Columns["sum_doc_date"].ColumnName = "Ημ/νία Τίτλου";
                dt.Columns["comments"].ColumnName = "Σχόλια";
                dt.Columns["gdocid"].ColumnName = "G_DOC_ID";
                dt.Columns["problem_descr"].ColumnName = "Παρατηρήσεις Σάρωσης";
                dt.Columns["issuer_name"].ColumnName = "Εκδότης";
                dt.Columns["doc_type_descr"].ColumnName = "Είδος τίτλου";
                dt.Columns["problem_code_descr"].ColumnName = "Προβλήματα Σαρωμένων Εγγράφων";
                dt.Columns["sum_status_descr"].ColumnName = "Κατάσταση Επεξεργασίας";
                dt.Columns["g_delivery_id"].ColumnName = "G_DELIVERY_ID";
                dt.Columns.Remove("g_doc_id");
                dt.Columns.Remove("sum_issuer_type");
                dt.Columns.Remove("sum_doc_type");
                dt.Columns.Remove("problem_code");
                dt.Columns.Remove("type");
                conn.Close();
                reportLogsString = reportLogsString + " Σωστή ανάγνωση από την βάση δεδομένων: " + currentDataBase + " για της perilipseis_paratiriseis_sarosis_saro.\n";
            }
            catch (Exception ex)
            {
                errorLogsString = errorLogsString + " (function: perilipseis_provlim_saro_eggraf_provlimatiki_saro) " + ex.Message + "\n";
            }
            return dt;
        }
        private DataTable lathi_provlimata_perigrafi_provlimatos_saro(String selectFieldName)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                connstring = "User Id=" + User_Id + ";Password=" + Password + ";Server=192.168.82.26;Port=5432;Database=" + currentDataBase + ";Pooling=false;Preload Reader=true;CommandTimeout=10000";
                NpgsqlConnection conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = @"SELECT i.g_eis_id, i.g_prop_id, i.g_doc_id, r.g_right_id, i.g_ben_id,
                CASE WHEN coalesce(i.for_ben,0)=0 then 'Όχι' else 'Ναι' END as for_ben
                , i.description, i.issue_category, i.issue_category cat_code, l.category_descr,
                username, check_description, CASE WHEN coalesce(corrected,0)=0 then 'Όχι' else 'Ναι' END as corrected, to_char(issue_date, 'DD/MM/YYYY') as issue_date_1, id
                FROM public.general_issues i
                LEFT JOIN ktinc.right r on i.g_prop_id=r.g_prop_id and i.g_prop_id=r.g_prop_id
                LEFT JOIN issue_category_lut l on i.issue_category=l.category_type
                and i.g_prop_id is not null
                where " + selectFieldName + " like '%ΣΑΡΩ%' or " + selectFieldName + " like '%σαρω%' or " + selectFieldName + " like '%σαρώ%' order by id desc";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
                da.Fill(ds);
                dt = ds.Tables[0];
                dt.Columns["g_eis_id"].ColumnName = "G_EIS_ID";
                dt.Columns["g_prop_id"].ColumnName = "G_PROP_ID";
                dt.Columns["g_doc_id"].ColumnName = "G_DOC_ID";
                dt.Columns["g_ben_id"].ColumnName = "G_BEN_ID";
                dt.Columns["username"].ColumnName = "Όνομα Χρήστη";
                dt.Columns["category_descr"].ColumnName = "Κατηγορία προβλήματος";
                dt.Columns["description"].ColumnName = "Περιγραφή προβλήματος";
                dt.Columns["check_description"].ColumnName = "Σχόλια Διόρθωσης";
                dt.Columns["issue_date_1"].ColumnName = "Ημ/νία Καταχώρισης";
                dt.Columns["for_ben"].ColumnName = "Αφορά Δικαιούχο";
                dt.Columns["corrected"].ColumnName = "Διορθωμένο";
                dt.Columns.Remove("id");
                dt.Columns.Remove("issue_category");
                dt.Columns.Remove("g_right_id");
                dt.Columns.Remove("cat_code");
                conn.Close();
                reportLogsString = reportLogsString + " Σωστή ανάγνωση από την βάση δεδομένων: " + currentDataBase + " για της lathi_provlimata_perigrafi_provlimatos_saro.\n";
            }
            catch (Exception ex)
            {
                errorLogsString = errorLogsString + " (function: perilipseis_provlim_saro_eggraf_provlimatiki_saro) " + ex.Message + "\n";
            }
            return dt;
        }
        private void writeTocsv(DataTable dt, String fileName)
        {
            try
            {
                StreamWriter csvSreamWriter = new StreamWriter(reportDir + "\\" + fileName, false, Encoding.Default);
                String currentLine = "";
                List<String> varcharFieldNames = new List<String>() { "G_EIS_ID", "G_DOC_ID", "G_PROP_ID", "g_ben_id", "G_DELIVERY_ID", "APPL_ID", "G_BEN_ID", "COVER_ID", "SUMMARY_ID" };
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    if (r == 0)
                    {
                        String headerLine = "";
                        for (int cIndex = 0; cIndex < dt.Columns.Count; cIndex++)
                        {
                            headerLine = headerLine + dt.Columns[cIndex].ColumnName + ";";
                        }
                        csvSreamWriter.WriteLine(headerLine);
                    }
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        if (varcharFieldNames.Contains(dt.Columns[c].ColumnName))
                        {
                            currentLine = currentLine + "'" + dt.Rows[r][c].ToString().Replace("\r\n", " ").Replace(";", "?").Replace("'", " ").Replace("\"", " ") + "'" + ";";
                        }
                        else
                        {
                            currentLine = currentLine + dt.Rows[r][c].ToString().Replace("\r\n", " ").Replace(";", "?").Replace("'", " ").Replace("\"", " ") + ";";
                        }
                    }
                    csvSreamWriter.WriteLine(currentLine);
                    currentLine = "";
                }
                csvSreamWriter.Close();
                reportLogsString = reportLogsString + " Σωστή εγγραφή του csv αρχείου " + fileName + ".\n";
            }
            catch (Exception ex)
            {
                errorLogsString = errorLogsString + " (function: perilipseis_provlim_saro_eggraf_provlimatiki_saro) " + ex.Message + "\n";
            }
        }
        public void CreateZip(String sourceName, String targetName)
        {
            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = @"C:\Program Files\7-Zip\7zG.exe";
            p.Arguments = "a -tzip \"" + targetName + "\" \"" + sourceName + "\"";//"a -tgzip \"" + targetName + "\" \"" + sourceName + "\" -mx=9";
            p.WindowStyle = ProcessWindowStyle.Hidden;
            Process x = Process.Start(p);
            x.WaitForExit();
        }

        public void deleteOldReports() 
        {
            DateTime currentDate = DateTime.Now;
            currentDate = currentDate.AddDays(-15);
            for(int i=0; i<30; i++)
            {
                currentDate = currentDate.AddDays(-1);
                String oldDirectory = Directory.GetCurrentDirectory() + "\\" + currentDate.ToString("yyyy-M-dd");
                String oldZipFile = Directory.GetCurrentDirectory() + "\\" + currentDate.ToString("yyyy-M-dd") + ".zip";
                if (Directory.Exists(oldDirectory))
                {
                    try
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(oldDirectory);
                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                        Directory.Delete(oldDirectory);
                        File.Delete(oldZipFile);
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }


    }
}
