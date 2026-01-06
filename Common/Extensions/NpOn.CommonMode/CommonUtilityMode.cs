using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Common.Extensions.NpOn.CommonMode;

public class CommonUtilityMode
{
    public static string GenerateNonce()
    {
        var random = new Random();
        return random.Next(1, 99999999).ToString().PadLeft(8);
    }

    public static bool IsIpAddress(string input)
    {
        return IPAddress.TryParse(input, out _);
    }

    public static string GenerateGuidAsString()
    {
        return Guid.CreateVersion7().ToString("N");
    }
    
    public static Guid GenerateGuid()
    {
        return Guid.CreateVersion7();
    }

    public static string GenerateCodeFromId(long id, int padLeft = 1)
    {
        string c = "123456789qwertyuiopasdfghjklzxcvbnm".ToUpper();
        string code = string.Empty;
        int length = c.Length;
        long index = id;
        while (true)
        {
            int lastIndex = (int)(index % length) - 1;
            if (lastIndex < 0)
            {
                lastIndex = c.Length - 1;
            }

            code = c[lastIndex] + code;
            if (index > length)
            {
                index = index / length;
                if (index < length)
                {
                    code = c[(int)index - 1] + code;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        code = code.PadLeft(padLeft, '0');
        return code;
    }

    public static string GenerateInvoiceSerialNoFromId(long id, int padLeft = 7)
    {
        return id.ToString().PadLeft(padLeft, '0');
    }

    public static bool IsMobileTabletBrowser(string userAgent)
    {
        return IsMobileBrowser(userAgent) || IsTabletBrowser(userAgent);
    }

    /// <summary>
    /// Nhận diện điện thoại di động theo User-Agent
    /// </summary>
    /// <param name="userAgent">User-Agent</param>
    /// <returns>true nếu client là điện thoại di động, false trong trường hợp ngược lại</returns>
    public static bool IsMobileBrowser(string userAgent)
    {
        try
        {
            Regex b =
                new Regex(
                    @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino|\bFB[\w_]+\/(Messenger|MESSENGER)|\bFB[\w_]+\/|\bMicroMessenger\/|\bPuffin|\bMiuiBrowser\/|\bInstagram|Zalo|Bphone",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex v =
                new Regex(
                    @"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return !String.IsNullOrWhiteSpace(userAgent) &&
                   (b.IsMatch(userAgent) || v.IsMatch(userAgent.Substring(0, 4)));
        }
        catch (Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// Nhận diện máy tính bảng theo User-Agent
    /// </summary>
    /// <param name="userAgent">User-Agent</param>
    /// <returns>True nếu là máy tính bảng, false trong trường hợp ngược lại</returns>
    public static bool IsTabletBrowser(string userAgent)
    {
        return !String.IsNullOrWhiteSpace(userAgent) &&
               Regex.IsMatch(userAgent,
                   @"iPad|GT-P|SCH-I800|Nexus\s(10|7)|Tablet|PlayBook|Xoom|Kindle|Silk|KFAPWI|Android(?!.*Mobile)",
                   RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetMethodName()
    {
        var st = new StackTrace(new StackFrame(1));
        return st.GetFrame(0).GetMethod().Name;
    }

    public static string GetFullMethodName(MethodBase? methodBase)
    {
        var fullMethodName = $"{methodBase?.ReflectedType?.FullName}.{methodBase?.Name}";
        try
        {
            var startMethodIndex = fullMethodName.LastIndexOf("+<", StringComparison.Ordinal);
            if (startMethodIndex < 0)
            {
                return fullMethodName;
            }

            var className = fullMethodName.Split("+")[0];
            var methodName =
                fullMethodName.Substring(startMethodIndex + 2, fullMethodName.Length - startMethodIndex - 2);
            methodName = methodName.Substring(0, methodName.IndexOf(">", StringComparison.Ordinal));
            methodName = methodName.Replace("<", "").Replace(">", "");
            return $"{className}.{methodName}";
        }
        catch (Exception e)
        {
            return fullMethodName;
        }
    }

    public static bool IsEmail(string emailaddress)
    {
        try
        {
            emailaddress = ReplaceSpace(emailaddress);
            MailAddress m = new MailAddress(emailaddress);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public const string EmailPattern =
        "^([0-9a-zA-Z]([-\\.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";

    public static bool EmailValid(ref string email)
    {
        email = email.Trim();
        try
        {
            if (!Regex.IsMatch(email, EmailPattern))
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public static bool MobileValid(ref string mobile)
    {
        if (string.IsNullOrEmpty(mobile))
        {
            return false;
        }

        if (!IsMobile(ref mobile)) return false;
        if (mobile.StartsWith("0"))
        {
            mobile = $"+84{mobile.Remove(0, 1)}";
        }

        return true;
    }

    public static bool IsMobile(ref string mobile)
    {
        try
        {
            mobile = ReplaceSpace(mobile);
            var isMatch = Regex.IsMatch(mobile, @"^[+]*[0-9]*$");
            if (!isMatch)
            {
                return false;
            }

            return true;
            // return IsViettel(ref mobile) || IsMobifone(ref mobile) || IsVinaphone(ref mobile) ||
            //        IsVietnamobile(ref mobile) || IsSphone(ref mobile) || IsGmobile(ref mobile) ||
            //        IsItelecom(ref mobile) || IsInternationalMobile(ref mobile);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static readonly string[] ViettelMobile =
        { "086", "096", "097", "098", "032", "033", "034", "035", "036", "037", "038", "039" };

    public static bool IsViettel(ref string mobile)
    {
        foreach (var s in ViettelMobile)
        {
            if (mobile.StartsWith(s))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] MobifoneMobile = { "089", "090", "093", "070", "079", "077", "076", "078" };

    public static bool IsMobifone(ref string mobile)
    {
        foreach (var s in MobifoneMobile)
        {
            if (mobile.StartsWith(s))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] VinaphoneMobile = { "088", "091", "094", "083", "084", "085", "081", "082" };

    public static bool IsVinaphone(ref string mobile)
    {
        foreach (var s in VinaphoneMobile)
        {
            if (mobile.StartsWith(s))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] VietnamobileMobile = { "092", "056", "058" };

    public static bool IsVietnamobile(ref string mobile)
    {
        foreach (var s in VietnamobileMobile)
        {
            if (mobile.StartsWith(s))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] SphoneMobile = { "095" };

    public static bool IsSphone(ref string mobile)
    {
        foreach (var s in SphoneMobile)
        {
            if (mobile.StartsWith(s))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] GmobileMobile = { "099", "059" };

    public static bool IsGmobile(ref string mobile)
    {
        foreach (var s in GmobileMobile)
        {
            if (mobile.StartsWith(s))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] ITelecom = { "087" };

    public static bool IsItelecom(ref string mobile)
    {
        foreach (var s in ITelecom)
        {
            if (mobile.StartsWith(s))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] InternationalMobile =
    {
        "+1",
        "+7",
        "+20",
        "+27",
        "+30",
        "+31",
        "+32",
        "+33",
        "+34",
        "+36",
        "+39",
        "+40",
        "+41",
        "+43",
        "+44",
        "+45",
        "+46",
        "+47",
        "+48",
        "+49",
        "+51",
        "+52",
        "+53",
        "+54",
        "+55",
        "+56",
        "+57",
        "+58",
        "+60",
        "+61",
        "+62",
        "+63",
        "+64",
        "+65",
        "+66",
        "+76",
        "+77",
        "+81",
        "+82",
        "+84",
        "+86",
        "+90",
        "+91",
        "+92",
        "+93",
        "+94",
        "+95",
        "+98",
        "+212",
        "+213",
        "+216",
        "+218",
        "+220",
        "+221",
        "+222",
        "+223",
        "+224",
        "+225",
        "+225",
        "+226",
        "+227",
        "+228",
        "+229",
        "+230",
        "+231",
        "+232",
        "+233",
        "+234",
        "+235",
        "+236",
        "+237",
        "+238",
        "+239",
        "+240",
        "+241",
        "+242",
        "+243",
        "+243",
        "+243",
        "+244",
        "+245",
        "+246",
        "+247",
        "+248",
        "+249",
        "+250",
        "+251",
        "+252",
        "+253",
        "+254",
        "+255",
        "+256",
        "+257",
        "+258",
        "+260",
        "+261",
        "+262",
        "+263",
        "+264",
        "+265",
        "+266",
        "+267",
        "+268",
        "+269",
        "+297",
        "+298",
        "+299",
        "+350",
        "+351",
        "+352",
        "+353",
        "+354",
        "+355",
        "+356",
        "+357",
        "+358",
        "+359",
        "+370",
        "+371",
        "+372",
        "+373",
        "+374",
        "+375",
        "+376",
        "+377",
        "+378",
        "+380",
        "+381",
        "+381",
        "+385",
        "+386",
        "+387",
        "+389",
        "+420",
        "+421",
        "+423",
        "+442",
        "+500",
        "+501",
        "+502",
        "+503",
        "+504",
        "+505",
        "+506",
        "+507",
        "+509",
        "+590",
        "+591",
        "+592",
        "+593",
        "+594",
        "+595",
        "+596",
        "+596",
        "+597",
        "+598",
        "+599",
        "+673",
        "+675",
        "+676",
        "+677",
        "+678",
        "+679",
        "+680",
        "+682",
        "+684",
        "+685",
        "+687",
        "+688",
        "+689",
        "+692",
        "+808",
        "+850",
        "+852",
        "+853",
        "+855",
        "+856",
        "+880",
        "+886",
        "+960",
        "+961",
        "+962",
        "+963",
        "+964",
        "+965",
        "+966",
        "+967",
        "+968",
        "+971",
        "+972",
        "+973",
        "+974",
        "+975",
        "+976",
        "+977",
        "+992",
        "+993",
        "+994",
        "+995",
        "+996",
        "+998",
        "+1264",
        "+1268",
        "+1284",
        "+1340",
        "+1345",
        "+1473",
        "+1649",
        "+1664",
        "+1670",
        "+1671",
        "+1758",
        "+1767",
        "+1784",
        "+1787",
        "+1939",
        "+1809",
        "+1829",
        "+1849",
        "+1868",
        "+1869",
        "+1876"
    };

    public static bool IsInternationalMobile(ref string mobile)
    {
        foreach (var s in InternationalMobile)
        {
            if (mobile.StartsWith(s))
            {
                return true;
            }
        }

        return false;
    }

    public static string ReplaceSpace(string txt)
    {
        if (string.IsNullOrEmpty(txt))
        {
            return string.Empty;
        }

        return Regex.Replace(txt, @"\s+", string.Empty);
    }

    public static string SetDomainCdn(string domain, string key, string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (path.StartsWith("http"))
        {
            return path;
        }

        if (path.StartsWith(key))
        {
            path = path.Remove(0, key.Length);
            return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
        }

        if (path.StartsWith($"/{key}"))
        {
            path = path.Remove(0, key.Length + 1);
            return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
        }

        return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
    }

    public static string SetDomainCdn(string domain, string key, string domainSystem, string keyStem, string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (path.StartsWith("https://vms-files.vietnamnet.vn/files"))
        {
            path = path.Replace("https://vms-files.vietnamnet.vn/files", string.Empty);
        }

        if (path.StartsWith("https://vms-files.vietnamnet.vn/cdn-files"))
        {
            path = path.Replace("https://vms-files.vietnamnet.vn/cdn-files", string.Empty);
        }

        if (path.StartsWith("https://vms-files.vietnamnet.vn"))
        {
            path = path.Replace("https://vms-files.vietnamnet.vn", string.Empty);
        }

        if (path.StartsWith("/cdn-files/files"))
        {
            path = path.Replace("/cdn-files/files", "/files");
        }

        if (path.StartsWith($"{key}/files/"))
        {
            path = path.Replace($"{key}/files/", "/");
        }

        if (path.StartsWith("http"))
        {
            return path;
        }

        if (path.StartsWith(key))
        {
            path = path.Remove(0, key.Length);
            return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
        }

        if (path.StartsWith($"/{key}"))
        {
            path = path.Remove(0, key.Length + 1);
            return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
        }

        if (path.StartsWith(keyStem))
        {
            path = path.Remove(0, keyStem.Length);
            return path.StartsWith("/") ? $"{domainSystem}{path}" : $"{domainSystem}/{path}";
        }

        if (path.StartsWith($"/{keyStem}"))
        {
            path = path.Remove(0, keyStem.Length + 1);
            return path.StartsWith("/") ? $"{domainSystem}{path}" : $"{domainSystem}/{path}";
        }

        return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
    }

    public static string SetDomainCdnByAdmin(string domain, string key, string domainSystem, string keyStem,
        string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (path.StartsWith("/cdn-files/files"))
        {
            path = path.Replace("/cdn-files/files", "/files");
        }

        if (path.StartsWith($"{key}/files/"))
        {
            path = path.Replace($"{key}/files/", "/");
        }

        if (path.StartsWith("http"))
        {
            return path;
        }

        if (path.StartsWith(key))
        {
            path = path.Remove(0, key.Length);
            return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
        }

        if (path.StartsWith($"/{key}"))
        {
            path = path.Remove(0, key.Length + 1);
            return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
        }

        if (path.StartsWith(keyStem))
        {
            path = path.Remove(0, keyStem.Length);
            return path.StartsWith("/") ? $"{domainSystem}{path}" : $"{domainSystem}/{path}";
        }

        if (path.StartsWith($"/{keyStem}"))
        {
            path = path.Remove(0, keyStem.Length + 1);
            return path.StartsWith("/") ? $"{domainSystem}{path}" : $"{domainSystem}/{path}";
        }

        return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
    }

    public static string SetDomainCdnByAdmin(string domain, string key, string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (path.StartsWith("http"))
        {
            return path;
        }

        if (path.StartsWith("/cdn-files/files"))
        {
            path = path.Replace("/cdn-files/files", "/files");
        }

        if (key.Length > 0)
        {
            if (path.StartsWith($"{key}/files/"))
            {
                path = path.Replace($"{key}/files/", "/");
            }

            if (path.StartsWith(key))
            {
                path = path.Remove(0, key.Length);
                return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
            }

            if (path.StartsWith($"/{key}"))
            {
                path = path.Remove(0, key.Length + 1);
                return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
            }
        }

        return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
    }

    public static string GetImageUrlPath(string domain, string key, string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (!path.StartsWith(domain))
        {
            return path;
        }

        if (path.StartsWith(key))
        {
            return path;
        }

        path = path.Remove(0, domain.Length);
        return path.StartsWith("/") ? $"{key}{path}" : $"{key}/{path}";
    }

    public static string SetDomainCdnAndSite(string domain, string key, string domainSystem, string keyStem,
        string path, int width)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        string url = SetDomainCdn(domain, key, domainSystem, keyStem, path);
        url += $"?width={width}";
        return url;
    }

    public static string SetDomainCdnAndSiteBackend(string domain, string key, string domainSystem,
        string keySystem, string path,
        int width)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        string url;
        if (path.StartsWith("http"))
        {
            url = path;
        }
        else if (path.StartsWith(key))
        {
            path = path.Remove(0, key.Length);
            url = path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
        }

        else if (path.StartsWith($"/{key}"))
        {
            path = path.Remove(0, key.Length + 1);
            url = path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
        }

        else if (path.StartsWith(keySystem))
        {
            path = path.Remove(0, keySystem.Length);
            url = path.StartsWith("/") ? $"{domainSystem}{path}" : $"{domainSystem}/{path}";
        }

        else if (path.StartsWith($"/{keySystem}"))
        {
            path = path.Remove(0, keySystem.Length + 1);
            url = path.StartsWith("/") ? $"{domainSystem}{path}" : $"{domainSystem}/{path}";
        }
        else
        {
            url = path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
        }

        if (width > 0)
        {
            if (url.StartsWith(domain) || url.StartsWith(domainSystem))
            {
                url += $"?width={width}";
            }
        }

        return url;
    }

    public static string SetCDNKey(string domain, string key, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(domain))
        {
            return string.Empty;
        }

        content = content.Replace(domain, key);
        return content;
    }

    public static string GetImageUrlPath(string domain, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        return path.Replace(domain, string.Empty);
    }

    public static string HomeUrl(string domain)
    {
        return $"{domain}/";
    }

    public static T[] FlagsEnumToArray<T>(Enum @enum, Type enumType)
    {
        List<T> enums = new List<T>();
        var values = Enum.GetValues(enumType);
        foreach (var value in values)
        {
            if (@enum.HasFlag((Enum)value))
            {
                enums.Add((T)value);
            }
        }

        return enums.ToArray();
    }

    public static string GenerateRandomPassword(int length, int uniqueLength, bool hasDigit, bool hasLowercase,
        bool hasNonAlphanumeric, bool hasUppercase)
    {
        string[] randomChars = new[]
        {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ", // uppercase 
            "abcdefghijkmnopqrstuvwxyz", // lowercase
            "0123456789", // digits
            "!@$?" // non-alphanumeric
        };
        Random rand = new Random(Environment.TickCount);
        List<char> chars = new List<char>();

        if (hasUppercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[0][rand.Next(0, randomChars[0].Length)]);

        if (hasLowercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[1][rand.Next(0, randomChars[1].Length)]);

        if (hasDigit)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[2][rand.Next(0, randomChars[2].Length)]);

        if (hasNonAlphanumeric)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[3][rand.Next(0, randomChars[3].Length)]);

        for (int i = chars.Count;
             i < length
             || chars.Distinct().Count() < uniqueLength;
             i++)
        {
            string rcs = randomChars[rand.Next(0, randomChars.Length)];
            chars.Insert(rand.Next(0, chars.Count),
                rcs[rand.Next(0, rcs.Length)]);
        }

        return new string(chars.ToArray());
    }

    public static string FormatPrice(int price)
    {
        if (price < 0)
        {
            price = 0;
        }

        return price.ToString("0,0.0", CultureInfo.InvariantCulture);
    }

    public static string FormatPrice(decimal price)
    {
        if (price < 0)
        {
            price = 0;
        }

        return price.ToString("0,0.0", CultureInfo.InvariantCulture);
    }

    public static string FormatMoney(decimal price)
    {
        return price.ToString("0,0.0", CultureInfo.InvariantCulture);
    }

    public static string FormatDate(DateTime? dateTime)
    {
        if (dateTime == null || dateTime == DateTime.MinValue)
        {
            return "";
        }

        return dateTime.Value.ToString("dd MMMM, yyyy");
    }

    public static string FormatShortDate(DateTime? dateTime)
    {
        if (dateTime == null || dateTime == DateTime.MinValue)
        {
            return "";
        }

        return dateTime.Value.ToString("dd/MM/yyyy");
    }

    public static string FormatDateAndTime(DateTime? dateTime)
    {
        if (dateTime == null || dateTime == DateTime.MinValue)
        {
            return ""; // 
        }

        return dateTime.Value.ToString("hh:mm dd MMMM, yyyy");
    }

    public static string ConvertIntToColumnNameExcel(int columnNumber)
    {
        StringBuilder retVal = new StringBuilder();
        int x;

        for (int n = (int)(Math.Log(25 * (columnNumber + 1)) / Math.Log(26)) - 1; n >= 0; n--)
        {
            x = (int)((Math.Pow(26, (n + 1)) - 1) / 25 - 1);
            if (columnNumber > x)
                retVal.Append(System.Convert.ToChar((int)(((columnNumber - x - 1) / Math.Pow(26, n)) % 26 + 65)));
        }

        return retVal.ToString();
    }

    public static int ConvertMeridianTimeToSeconds(string time)
    {
        if (string.IsNullOrEmpty(time))
        {
            return 0;
        }

        var timeArr = time.Substring(0, time.IndexOf(" ")).Split(':');
        int.TryParse(timeArr[0], out int hours);
        int.TryParse(timeArr[1], out int minutes);
        if (time.Contains("PM"))
        {
            hours += hours % 12;
        }

        return hours * 3600 + minutes * 60;
    }

    public static int ConvertTimeToSeconds(string time)
    {
        int seconds = 0;
        TimeSpan result;
        if (TimeSpan.TryParse(time, out result))
        {
            seconds = (int)result.TotalSeconds;
        }

        return seconds;
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public static bool IsValidVietnamesePhoneNumber(string phoneNumber)
    {
        var domesticPattern = @"^0[3|5|7|8|9]\d{8}$";
        var internationalPattern = @"^\+84[3|5|7|8|9]\d{8}$";
        var isDomesticValid = Regex.IsMatch(phoneNumber, domesticPattern);
        var isInternationalValid = Regex.IsMatch(phoneNumber, internationalPattern);
        return isDomesticValid || isInternationalValid;
    }


    public static string FormatPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            phoneNumber = string.Empty;
        }

        phoneNumber = phoneNumber.Trim().Replace(" ", string.Empty);

        if (phoneNumber.StartsWith("84"))
        {
            phoneNumber = "+84" + phoneNumber.Substring(2);
        }

        if (phoneNumber.StartsWith("+840"))
        {
            var temp = phoneNumber.Substring(3).TrimStart('0');
            phoneNumber = "+84" + temp;
        }

        if (phoneNumber.StartsWith("0"))
        {
            phoneNumber = "+84" + phoneNumber.TrimStart('0');
        }

        return phoneNumber;
    }

    public static string UserGetShortName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "";
        var shortName = "";
        Regex regex = new Regex(@"\b(\w)");
        var matches = regex.Matches(fullName);
        if (matches.Count > 1)
        {
            shortName = matches[0].Value + "" + matches[matches.Count - 1].Value;
        }
        else
        {
            shortName = string.Join("", matches.Select(s => s.Value));
        }

        return shortName.ToUpper();
    }

    public static string GetRandomString(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string GeneratePassword(int lowercase = 4, int uppercase = 2, int numerics = 2,
        int specialNumber = 2)
    {
        const string lowerString = "abcdefghijklmnopqursuvwxyz";
        const string upperString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string number = "0123456789";
        const string specials = @"!@£$%^&*()#€";

        var random = new Random();

        var generated = "!";
        for (int i = 1; i <= lowercase; i++)
            generated = generated.Insert(
                random.Next(generated.Length),
                lowerString[random.Next(lowerString.Length - 1)].ToString()
            );

        for (int i = 1; i <= uppercase; i++)
            generated = generated.Insert(
                random.Next(generated.Length),
                upperString[random.Next(upperString.Length - 1)].ToString()
            );

        for (int i = 1; i <= numerics; i++)
            generated = generated.Insert(
                random.Next(generated.Length),
                number[random.Next(number.Length - 1)].ToString()
            );

        for (int i = 1; i <= specialNumber; i++)
            generated = generated.Insert(
                random.Next(generated.Length),
                specials[random.Next(specials.Length - 1)].ToString()
            );

        return generated.Replace("!", string.Empty);
    }

    public static string ExtractText(string html, int stringLength = 100)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        var reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
        var contentString = reg.Replace(html, " ");
        contentString = HttpUtility.HtmlDecode(contentString);
        if (contentString.Length > stringLength)
        {
            return contentString.Substring(0, stringLength);
        }

        return contentString;
    }

    public static (long, string, bool)[] FromEnumSelectMutiple(Type enumType, long allStatus,
        bool addDefaultItem = true)
    {
        var values = Enum.GetValues(enumType);
        int length = values.Length;
        if (addDefaultItem)
        {
            length++;
        }

        (long, string, bool)[] keyValueModels = new (long, string, bool)[length];
        int i = 0;

        foreach (var value in values)
        {
            keyValueModels[i] = (((long)value), ((Enum)value).ToString(), (allStatus & (long)value) == (long)value);
            i++;
        }

        return keyValueModels;
    }

    public static (long, string)[] FromEnumTypeLong(Type enumType)
    {
        var values = Enum.GetValues(enumType);
        int length = values.Length;

        (long, string)[] keyValueModels = new (long, string)[length];
        int i = 0;

        foreach (var value in values)
        {
            keyValueModels[i] = (((long)value), ((Enum)value).ToString());
            i++;
        }

        return keyValueModels;
    }

    public static (int, string)[] FromEnumTypeInt(Type enumType)
    {
        var values = Enum.GetValues(enumType);
        int length = values.Length;

        (int, string)[] keyValueModels = new (int, string)[length];
        int i = 0;

        foreach (var value in values)
        {
            keyValueModels[i] = (((int)value), ((Enum)value).ToString());
            i++;
        }

        return keyValueModels;
    }

    public static string SetDomainWebSite(string domain, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        if (path.StartsWith("http"))
        {
            return path;
        }

        return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
    }

    public static string
        HashToFixedLength(string input, int length) // đang được sử dụng trong việc tạo id trong OldSystem
    {
        if (string.IsNullOrEmpty(input))
            return new string(' ', length);
        using var md5 = MD5.Create(); // Create MD5 hash
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder sb = new StringBuilder(); // Convert the byte array to a hexadecimal string
        foreach (byte b in hashBytes)
            sb.Append(b.ToString("x2"));
        string hashString = sb.ToString();
        if (hashString.Length > length)
            return hashString.Substring(0, length);
        return hashString;
    }

    public class Crc64
    {
        private static readonly ulong[] Table =
        {
            0x0000000000000000, 0x7ad870c830358979,
            0xf5b0e190606b12f2, 0x8f689158505e9b8b,
            0xc038e5739841b68f, 0xbae095bba8743ff6,
            0x358804e3f82aa47d, 0x4f50742bc81f2d04,
            0xab28ecb46814fe75, 0xd1f09c7c5821770c,
            0x5e980d24087fec87, 0x24407dec384a65fe,
            0x6b1009c7f05548fa, 0x11c8790fc060c183,
            0x9ea0e857903e5a08, 0xe478989fa00bd371,
            0x7d08ff3b88be6f81, 0x07d08ff3b88be6f8,
            0x88b81eabe8d57d73, 0xf2606e63d8e0f40a,
            0xbd301a4810ffd90e, 0xc7e86a8020ca5077,
            0x4880fbd87094cbfc, 0x32588b1040a14285,
            0xd620138fe0aa91f4, 0xacf86347d09f188d,
            0x2390f21f80c18306, 0x594882d7b0f40a7f,
            0x1618f6fc78eb277b, 0x6cc0863448deae02,
            0xe3a8176c18803589, 0x997067a428b5bcf0,
            0xfa11fe77117cdf02, 0x80c98ebf2149567b,
            0x0fa11fe77117cdf0, 0x75796f2f41224489,
            0x3a291b04893d698d, 0x40f16bccb908e0f4,
            0xcf99fa94e9567b7f, 0xb5418a5cd963f206,
            0x513912c379682177, 0x2be1620b495da80e,
            0xa489f35319033385, 0xde51839b2936bafc,
            0x9101f7b0e12997f8, 0xebd98778d11c1e81,
            0x64b116208142850a, 0x1e6966e8b1770c73,
            0x8719014c99c2b083, 0xfdc17184a9f739fa,
            0x72a9e0dcf9a9a271, 0x08719014c99c2b08,
            0x4721e43f0183060c, 0x3df994f731b68f75,
            0xb29105af61e814fe, 0xc849756751dd9d87,
            0x2c31edf8f1d64ef6, 0x56e99d30c1e3c78f,
            0xd9810c6891bd5c04, 0xa3597ca0a188d57d,
            0xec09088b6997f879, 0x96d1784359a27100,
            0x19b9e91b09fcea8b, 0x636199d339c963f2,
            0xdf7adabd7a6e2d6f, 0xa5a2aa754a5ba416,
            0x2aca3b2d1a053f9d, 0x50124be52a30b6e4,
            0x1f423fcee22f9be0, 0x659a4f06d21a1299,
            0xeaf2de5e82448912, 0x902aae96b271006b,
            0x74523609127ad31a, 0x0e8a46c1224f5a63,
            0x81e2d7997211c1e8, 0xfb3aa75142244891,
            0xb46ad37a8a3b6595, 0xceb2a3b2ba0eecec,
            0x41da32eaea507767, 0x3b024222da65fe1e,
            0xa2722586f2d042ee, 0xd8aa554ec2e5cb97,
            0x57c2c41692bb501c, 0x2d1ab4dea28ed965,
            0x624ac0f56a91f461, 0x1892b03d5aa47d18,
            0x97fa21650afae693, 0xed2251ad3acf6fea,
            0x095ac9329ac4bc9b, 0x7382b9faaaf135e2,
            0xfcea28a2faafae69, 0x8632586aca9a2710,
            0xc9622c4102850a14, 0xb3ba5c8932b0836d,
            0x3cd2cdd162ee18e6, 0x460abd1952db919f,
            0x256b24ca6b12f26d, 0x5fb354025b277b14,
            0xd0dbc55a0b79e09f, 0xaa03b5923b4c69e6,
            0xe553c1b9f35344e2, 0x9f8bb171c366cd9b,
            0x10e3202993385610, 0x6a3b50e1a30ddf69,
            0x8e43c87e03060c18, 0xf49bb8b633338561,
            0x7bf329ee636d1eea, 0x012b592653589793,
            0x4e7b2d0d9b47ba97, 0x34a35dc5ab7233ee,
            0xbbcbcc9dfb2ca865, 0xc113bc55cb19211c,
            0x5863dbf1e3ac9dec, 0x22bbab39d3991495,
            0xadd33a6183c78f1e, 0xd70b4aa9b3f20667,
            0x985b3e827bed2b63, 0xe2834e4a4bd8a21a,
            0x6debdf121b863991, 0x1733afda2bb3b0e8,
            0xf34b37458bb86399, 0x8993478dbb8deae0,
            0x06fbd6d5ebd3716b, 0x7c23a61ddbe6f812,
            0x3373d23613f9d516, 0x49aba2fe23cc5c6f,
            0xc6c333a67392c7e4, 0xbc1b436e43a74e9d,
            0x95ac9329ac4bc9b5, 0xef74e3e19c7e40cc,
            0x601c72b9cc20db47, 0x1ac40271fc15523e,
            0x5594765a340a7f3a, 0x2f4c0692043ff643,
            0xa02497ca54616dc8, 0xdafce7026454e4b1,
            0x3e847f9dc45f37c0, 0x445c0f55f46abeb9,
            0xcb349e0da4342532, 0xb1eceec59401ac4b,
            0xfebc9aee5c1e814f, 0x8464ea266c2b0836,
            0x0b0c7b7e3c7593bd, 0x71d40bb60c401ac4,
            0xe8a46c1224f5a634, 0x927c1cda14c02f4d,
            0x1d148d82449eb4c6, 0x67ccfd4a74ab3dbf,
            0x289c8961bcb410bb, 0x5244f9a98c8199c2,
            0xdd2c68f1dcdf0249, 0xa7f41839ecea8b30,
            0x438c80a64ce15841, 0x3954f06e7cd4d138,
            0xb63c61362c8a4ab3, 0xcce411fe1cbfc3ca,
            0x83b465d5d4a0eece, 0xf96c151de49567b7,
            0x76048445b4cbfc3c, 0x0cdcf48d84fe7545,
            0x6fbd6d5ebd3716b7, 0x15651d968d029fce,
            0x9a0d8ccedd5c0445, 0xe0d5fc06ed698d3c,
            0xaf85882d2576a038, 0xd55df8e515432941,
            0x5a3569bd451db2ca, 0x20ed197575283bb3,
            0xc49581ead523e8c2, 0xbe4df122e51661bb,
            0x3125607ab548fa30, 0x4bfd10b2857d7349,
            0x04ad64994d625e4d, 0x7e7514517d57d734,
            0xf11d85092d094cbf, 0x8bc5f5c11d3cc5c6,
            0x12b5926535897936, 0x686de2ad05bcf04f,
            0xe70573f555e26bc4, 0x9ddd033d65d7e2bd,
            0xd28d7716adc8cfb9, 0xa85507de9dfd46c0,
            0x273d9686cda3dd4b, 0x5de5e64efd965432,
            0xb99d7ed15d9d8743, 0xc3450e196da80e3a,
            0x4c2d9f413df695b1, 0x36f5ef890dc31cc8,
            0x79a59ba2c5dc31cc, 0x037deb6af5e9b8b5,
            0x8c157a32a5b7233e, 0xf6cd0afa9582aa47,
            0x4ad64994d625e4da, 0x300e395ce6106da3,
            0xbf66a804b64ef628, 0xc5bed8cc867b7f51,
            0x8aeeace74e645255, 0xf036dc2f7e51db2c,
            0x7f5e4d772e0f40a7, 0x05863dbf1e3ac9de,
            0xe1fea520be311aaf, 0x9b26d5e88e0493d6,
            0x144e44b0de5a085d, 0x6e963478ee6f8124,
            0x21c640532670ac20, 0x5b1e309b16452559,
            0xd476a1c3461bbed2, 0xaeaed10b762e37ab,
            0x37deb6af5e9b8b5b, 0x4d06c6676eae0222,
            0xc26e573f3ef099a9, 0xb8b627f70ec510d0,
            0xf7e653dcc6da3dd4, 0x8d3e2314f6efb4ad,
            0x0256b24ca6b12f26, 0x788ec2849684a65f,
            0x9cf65a1b368f752e, 0xe62e2ad306bafc57,
            0x6946bb8b56e467dc, 0x139ecb4366d1eea5,
            0x5ccebf68aecec3a1, 0x2616cfa09efb4ad8,
            0xa97e5ef8cea5d153, 0xd3a62e30fe90582a,
            0xb0c7b7e3c7593bd8, 0xca1fc72bf76cb2a1,
            0x45775673a732292a, 0x3faf26bb9707a053,
            0x70ff52905f188d57, 0x0a2722586f2d042e,
            0x854fb3003f739fa5, 0xff97c3c80f4616dc,
            0x1bef5b57af4dc5ad, 0x61372b9f9f784cd4,
            0xee5fbac7cf26d75f, 0x9487ca0fff135e26,
            0xdbd7be24370c7322, 0xa10fceec0739fa5b,
            0x2e675fb4576761d0, 0x54bf2f7c6752e8a9,
            0xcdcf48d84fe75459, 0xb71738107fd2dd20,
            0x387fa9482f8c46ab, 0x42a7d9801fb9cfd2,
            0x0df7adabd7a6e2d6, 0x772fdd63e7936baf,
            0xf8474c3bb7cdf024, 0x829f3cf387f8795d,
            0x66e7a46c27f3aa2c, 0x1c3fd4a417c62355,
            0x935745fc4798b8de, 0xe98f353477ad31a7,
            0xa6df411fbfb21ca3, 0xdc0731d78f8795da,
            0x536fa08fdfd90e51, 0x29b7d047efec8728,
        };

        public static ulong Compute(byte[] s, ulong crc = 0)
        {
            for (int j = 0; j < s.Length; j++)
            {
                crc = Crc64.Table[(byte)(crc ^ s[j])] ^ (crc >> 8);
            }

            return crc;
        }

        public static ulong Compute(string s, ulong crc = 0)
        {
            var byteData = Encoding.UTF8.GetBytes(s);
            return Compute(byteData, crc);
        }
    }

    public static string GetStringBetween(string strSource, string strStart, string strEnd)
    {
        if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        {
            int Start, End;
            Start = strSource.IndexOf(strStart, 0) + strStart.Length;
            End = strSource.IndexOf(strEnd, Start);
            return strSource.Substring(Start, End - Start);
        }

        return "";
    }

    public static string GetExcelColumnName(int columnNumber)
    {
        string columnName = "";

        while (columnNumber > 0)
        {
            int modulo = (columnNumber - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            columnNumber = (columnNumber - modulo) / 26;
        }

        return columnName;
    }

    public static string ChangeToVietNamDate(DateTime? dt)
    {
        string strVietNameDate = "";
        try
        {
            string t = dt.Value.DayOfWeek.ToString();
            string ngay = "";
            switch (t.ToLower())
            {
                case "monday":
                    ngay = "Thứ hai";
                    break;
                case "tuesday":
                    ngay = "Thứ ba";
                    break;
                case "wednesday":
                    ngay = "Thứ tư";
                    break;
                case "thursday":
                    ngay = "Thứ năm";
                    break;
                case "friday":
                    ngay = "Thứ sáu";
                    break;
                case "saturday":
                    ngay = "Thứ bảy";
                    break;
                case "sunday":
                    ngay = "Chủ nhật";
                    break;
            }

            strVietNameDate = ngay + ", " + dt.Value.ToString("dd/MM/yyyy");
        }
        catch
        {
        }

        return strVietNameDate;
    }

    public static DateTime StringToDate(string? date)
    {
        try
        {
            var returnValue = DateTime.MinValue;
            if (!string.IsNullOrEmpty(date))
                returnValue = DateTime.Parse(date);
            return returnValue;
        }
        catch (Exception e)
        {
            return DateTime.MinValue;
        }
    }

    public static string SetDomain(string? domain, string? path)
    {
        if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        return path.StartsWith("/") ? $"{domain}{path}" : $"{domain}/{path}";
    }

    public static bool IsUnicodeDungSan(string value)
    {
        foreach (var s in UnicodeTable)
        {
            if (value.IndexOf(s.Value) > 0)
            {
                return true;
            }
        }

        return false;
    }

    public static Dictionary<string, string> UnicodeTable = new() // unicode tổ hợp, unicode dựng sẵn
    {
        { "\u0065\u0309", "\u1EBB" }, // ẻ
        { "\u0065\u0301", "\u00E9" }, // é
        { "\u0065\u0300", "\u00E8" }, // è
        { "\u0065\u0323", "\u1EB9" }, // ẹ
        { "\u0065\u0303", "\u1EBD" }, // ẽ
        { "\u00EA\u0309", "\u1EC3" }, // ể
        { "\u00EA\u0301", "\u1EBF" }, // ế
        { "\u00EA\u0300", "\u1EC1" }, // ề
        { "\u00EA\u0323", "\u1EC7" }, // ệ
        { "\u00EA\u0303", "\u1EC5" }, // ễ
        { "\u0079\u0309", "\u1EF7" }, // ỷ
        { "\u0079\u0301", "\u00FD" }, // ý
        { "\u0079\u0300", "\u1EF3" }, // ỳ
        { "\u0079\u0323", "\u1EF5" }, // ỵ
        { "\u0079\u0303", "\u1EF9" }, // ỹ
        { "\u0075\u0309", "\u1EE7" }, // ủ
        { "\u0075\u0301", "\u00FA" }, // ú
        { "\u0075\u0300", "\u00F9" }, // ù
        { "\u0075\u0323", "\u1EE5" }, // ụ
        { "\u0075\u0303", "\u0169" }, // ũ
        { "\u01B0\u0309", "\u1EED" }, // ử
        { "\u01B0\u0301", "\u1EE9" }, // ứ
        { "\u01B0\u0300", "\u1EEB" }, // ừ
        { "\u01B0\u0323", "\u1EF1" }, // ự
        { "\u01B0\u0303", "\u1EEF" }, // ữ
        { "\u0069\u0309", "\u1EC9" }, // ỉ
        { "\u0069\u0301", "\u00ED" }, // í
        { "\u0069\u0300", "\u00EC" }, // ì
        { "\u0069\u0323", "\u1ECB" }, // ị
        { "\u0069\u0303", "\u0129" }, // ĩ
        { "\u006F\u0309", "\u1ECF" }, // ỏ
        { "\u006F\u0301", "\u00F3" }, // ó
        { "\u006F\u0300", "\u00F2" }, // ò
        { "\u006F\u0323", "\u1ECD" }, // ọ
        { "\u006F\u0303", "\u00F5" }, // õ
        { "\u01A1\u0309", "\u1EDF" }, // ở
        { "\u01A1\u0301", "\u1EDB" }, // ớ
        { "\u01A1\u0300", "\u1EDD" }, // ờ
        { "\u01A1\u0323", "\u1EE3" }, // ợ
        { "\u01A1\u0303", "\u1EE1" }, // ỡ
        { "\u00F4\u0309", "\u1ED5" }, // ổ
        { "\u00F4\u0301", "\u1ED1" }, // ố
        { "\u00F4\u0300", "\u1ED3" }, // ồ
        { "\u00F4\u0323", "\u1ED9" }, // ộ
        { "\u00F4\u0303", "\u1ED7" }, // ỗ
        { "\u0061\u0309", "\u1EA3" }, // ả
        { "\u0061\u0301", "\u00E1" }, // á
        { "\u0061\u0300", "\u00E0" }, // à
        { "\u0061\u0323", "\u1EA1" }, // ạ
        { "\u0061\u0303", "\u00E3" }, // ã
        { "\u0103\u0309", "\u1EB3" }, // ẳ
        { "\u0103\u0301", "\u1EAF" }, // ắ
        { "\u0103\u0300", "\u1EB1" }, // ằ
        { "\u0103\u0323", "\u1EB7" }, // ặ
        { "\u0103\u0303", "\u1EB5" }, // ẵ
        { "\u00E2\u0309", "\u1EA9" }, // ẩ
        { "\u00E2\u0301", "\u1EA5" }, // ấ
        { "\u00E2\u0300", "\u1EA7" }, // ầ
        { "\u00E2\u0323", "\u1EAD" }, // ậ
        { "\u00E2\u0303", "\u1EAB" }, // ẫ
        { "\u0045\u0309", "\u1EBA" }, // Ẻ
        { "\u0045\u0301", "\u00C9" }, // É
        { "\u0045\u0300", "\u00C8" }, // È
        { "\u0045\u0323", "\u1EB8" }, // Ẹ
        { "\u0045\u0303", "\u1EBC" }, // Ẽ
        { "\u00CA\u0309", "\u1EC2" }, // Ể
        { "\u00CA\u0301", "\u1EBE" }, // Ế
        { "\u00CA\u0300", "\u1EC0" }, // Ề
        { "\u00CA\u0323", "\u1EC6" }, // Ệ
        { "\u00CA\u0303", "\u1EC4" }, // Ễ
        { "\u0059\u0309", "\u1EF6" }, // Ỷ
        { "\u0059\u0301", "\u00DD" }, // Ý
        { "\u0059\u0300", "\u1EF2" }, // Ỳ
        { "\u0059\u0323", "\u1EF4" }, // Ỵ
        { "\u0059\u0303", "\u1EF8" }, // Ỹ
        { "\u0055\u0309", "\u1EE6" }, // Ủ
        { "\u0055\u0301", "\u00DA" }, // Ú
        { "\u0055\u0300", "\u00D9" }, // Ù
        { "\u0055\u0323", "\u1EE4" }, // Ụ
        { "\u0055\u0303", "\u0168" }, // Ũ
        { "\u01AF\u0309", "\u1EEC" }, // Ử
        { "\u01AF\u0301", "\u1EE8" }, // Ứ
        { "\u01AF\u0300", "\u1EEA" }, // Ừ
        { "\u01AF\u0323", "\u1EF0" }, // Ự
        { "\u01AF\u0303", "\u1EEE" }, // Ữ
        { "\u0049\u0309", "\u1EC8" }, // Ỉ
        { "\u0049\u0301", "\u00CD" }, // Í
        { "\u0049\u0300", "\u00CC" }, // Ì
        { "\u0049\u0323", "\u1ECA" }, // Ị
        { "\u0049\u0303", "\u0128" }, // Ĩ
        { "\u004F\u0309", "\u1ECE" }, // Ỏ
        { "\u004F\u0301", "\u00D3" }, // Ó
        { "\u004F\u0300", "\u00D2" }, // Ò
        { "\u004F\u0323", "\u1ECC" }, // Ọ
        { "\u004F\u0303", "\u00D5" }, // Õ
        { "\u01A0\u0309", "\u1EDE" }, // Ở
        { "\u01A0\u0301", "\u1EDA" }, // Ớ
        { "\u01A0\u0300", "\u1EDC" }, // Ờ
        { "\u01A0\u0323", "\u1EE2" }, // Ợ
        { "\u01A0\u0303", "\u1EE0" }, // Ỡ
        { "\u00D4\u0309", "\u1ED4" }, // Ổ
        { "\u00D4\u0301", "\u1ED0" }, // Ố
        { "\u00D4\u0300", "\u1ED2" }, // Ồ
        { "\u00D4\u0323", "\u1ED8" }, // Ộ
        { "\u00D4\u0303", "\u1ED6" }, // Ỗ
        { "\u0041\u0309", "\u1EA2" }, // Ả
        { "\u0041\u0301", "\u00C1" }, // Á
        { "\u0041\u0300", "\u00C0" }, // À
        { "\u0041\u0323", "\u1EA0" }, // Ạ
        { "\u0041\u0303", "\u00C3" }, // Ã
        { "\u0102\u0309", "\u1EB2" }, // Ẳ
        { "\u0102\u0301", "\u1EAE" }, // Ắ
        { "\u0102\u0300", "\u1EB0" }, // Ằ
        { "\u0102\u0323", "\u1EB6" }, // Ặ
        { "\u0102\u0303", "\u1EB4" }, // Ẵ
        { "\u00C2\u0309", "\u1EA8" }, // Ẩ
        { "\u00C2\u0301", "\u1EA4" }, // Ấ
        { "\u00C2\u0300", "\u1EA6" }, // Ầ
        { "\u00C2\u0323", "\u1EAC" }, // Ậ
        { "\u00C2\u0303", "\u1EAA" } // Ẫ
    };


    public static Tuple<string, string> GetDomainAndArticleIdFromUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return new Tuple<string, string>(string.Empty, string.Empty);
        url = url.Trim();
        // https://vietnamnet.vn/mai-doanh-thu-hon-400-ti-box-office-vietnam-noi-gi-2251945.html
        string pattern = @"^(https?://)(.*)/(.*)-(.*)(\.html)$";

        Regex regex = new Regex(pattern);

        Match match = regex.Match(url);

        if (match.Success)
        {
            var domain = match.Groups[2].Value;
            var articleId = match.Groups[4].Value;

            // https://vietnamnet.vn/viet-nam-nhap-khau-o-to-giam-hon-50-so-voi-thang-1-2023-pre2252036.html
            if (articleId.StartsWith("pre")) articleId = articleId.Substring(3);
            return new Tuple<string, string>(domain, articleId);
        }

        return new Tuple<string, string>(string.Empty, string.Empty);
    }

    public static (long?, string) GetInt64HashCode(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return (null, "text is empty");
        }

        try
        {
            byte[] byteContents = Encoding.Unicode.GetBytes(text);
            using var hash = SHA256.Create();
            byte[] hashText = hash.ComputeHash(byteContents);
            var hashCodeStart = BitConverter.ToInt64(hashText, 0);
            var hashCodeMedium = BitConverter.ToInt64(hashText, 8);
            var hashCodeEnd = BitConverter.ToInt64(hashText, 24);
            var hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            return (hashCode, string.Empty);
        }
        catch (Exception e)
        {
            return (null, e.Message);
        }
    }

    public static (bool?, string) CheckIpAddressIsInRange(string checkIp, string cidrIp)
    {
        try
        {
            var checkIpSplit = checkIp.Split('.');
            if (checkIpSplit.Length != 4)
            {
                return (false, $"checkIp {checkIp} is not in valid IPV4 format a.b.c.d");
            }

            var cidrIpParts = cidrIp.Split('/');
            if (cidrIpParts.Length != 2)
            {
                return (false, $"cidrIp {cidrIp} is not in the correct format a.b.c.d/e");
            }

            var cidrIpSplit = cidrIpParts[0].Split('.');
            if (cidrIpSplit.Length != 4)
            {
                return (false, $"cidrIp {cidrIp} is not in the correct format a.b.c.d/e");
            }

            if (!int.TryParse(cidrIpParts[1], out var netmaskBitCount))
            {
                return (false, $"cidrIp {cidrIp} is not in the correct format a.b.c.d/e");
            }

            if (netmaskBitCount is < 0 or > 32)
            {
                return (false, $"netmaskBitCount {netmaskBitCount} is invalid, must be in range 0-32");
            }

            var ipAddress = ParseIPv4Addresses(checkIp)[0];
            var cidrAddress = ParseIPv4Addresses(cidrIpParts[0])[0];
            var checkResult = IpAddressIsInRange(ipAddress, cidrAddress, netmaskBitCount);
            return (checkResult, string.Empty);
        }
        catch (Exception e)
        {
            return (null, e.Message);
        }
    }

    private static bool IpAddressIsInRange(IPAddress checkIp, IPAddress cidrIp, int netmaskBitCount)
    {
        var ipAddressBytes = BitConverter.ToInt32(checkIp.GetAddressBytes(), 0);
        var cidrAddressBytes = BitConverter.ToInt32(cidrIp.GetAddressBytes(), 0);
        var cidrMaskBytes = IPAddress.HostToNetworkOrder(-1 << (32 - netmaskBitCount));
        var ipIsInRange = (ipAddressBytes & cidrMaskBytes) == (cidrAddressBytes & cidrMaskBytes);
        return ipIsInRange;
    }

    private static List<IPAddress> ParseIPv4Addresses(string input)
    {
        const string ipV4Pattern =
            @"(?:(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d)\.){3}(?:1\d\d|2[0-5][0-5]|2[0-4]\d|0?[1-9]\d|0?0?\d)";

        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input string must not be null");
        }

        var ips = new List<IPAddress>();
        var regex = new Regex(ipV4Pattern);
        foreach (Match match in regex.Matches(input))
        {
            var ip = ParseSingleIPv4Address(match.Value);
            ips.Add(ip);
        }

        return ips;
    }

    private static IPAddress ParseSingleIPv4Address(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input string must not be null");
        }

        var addressBytesSplit = input.Trim().Split('.').ToList();
        if (addressBytesSplit.Count != 4)
        {
            throw new ArgumentException($"Input string {input} was not in valid IPV4 format \"a.b.c.d\"");
        }

        var addressBytes = new byte[4];
        foreach (var i in Enumerable.Range(0, addressBytesSplit.Count))
        {
            if (!int.TryParse(addressBytesSplit[i], out var parsedInt))
            {
                throw new FormatException($"Unable to parse integer from {addressBytesSplit[i]}");
            }

            if (parsedInt is < 0 or > 255)
            {
                throw new ArgumentOutOfRangeException($"{parsedInt} not within required IP address range [0,255]");
            }

            addressBytes[i] = (byte)parsedInt;
        }

        return new IPAddress(addressBytes);
    }

    public static (int, string, bool)[] FromEnumSelectMultiple(Type enumType, int allStatus)
    {
        var values = Enum.GetValues(enumType);
        int length = values.Length;
        (int, string, bool)[] keyValueModels = new (int, string, bool)[length];
        int i = 0;

        foreach (var value in values)
        {
            keyValueModels[i] = (((int)value), ((Enum)value).ToString(), (allStatus & (int)value) == (int)value);
            i++;
        }

        return keyValueModels;
    }

    public static (long, string, bool)[] FromEnumSelectMultiple(Type enumType, long allStatus)
    {
        var values = Enum.GetValues(enumType);
        int length = values.Length;
        (long, string, bool)[] keyValueModels = new (long, string, bool)[length];
        int i = 0;

        foreach (var value in values)
        {
            keyValueModels[i] = (((long)value), ((Enum)value).ToString(), (allStatus & (long)value) == (long)value);
            i++;
        }

        return keyValueModels;
    }

    public static (int, string, bool)[] FromEnumTypeIntSelectMultiple(Type enumType, long allStatus)
    {
        var values = Enum.GetValues(enumType);
        int length = values.Length;
        (int, string, bool)[] keyValueModels = new (int, string, bool)[length];
        int i = 0;

        foreach (var value in values)
        {
            keyValueModels[i] = (((int)value), ((Enum)value).ToString(), (allStatus & (int)value) == (int)value);
            i++;
        }

        return keyValueModels;
    }

    public static string TruncateString(string text, int maxWords = 30)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= maxWords) return text;

        return string.Join(" ", words.Take(maxWords)) + "...";
    }

    public static double? CalculateDistanceBetween2Coordinates(double? lat1, double? lon1, double? lat2, double? lon2)
    {
        if (lat1 == null || lon1 == null || lat2 == null || lon2 == null)
        {
            return null;
        }

        const double R = 6371; // Bán kính trái đất

        var latRad1 = double.DegreesToRadians(lat1.Value);
        var latRad2 = double.DegreesToRadians(lat2.Value);
        var deltaLat = double.DegreesToRadians(lat2.Value - lat1.Value);
        var deltaLon = double.DegreesToRadians(lon2.Value - lon1.Value);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(latRad1) * Math.Cos(latRad2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var distance = R * c;
        return Math.Round(distance, 2);
    }

    public static int CalculateFullYears(DateTime startDate, DateTime endDate)
    {
        int years = endDate.Year - startDate.Year;

        // Nếu chưa tới ngày kỷ niệm trong năm hiện tại thì trừ đi 1
        if (endDate.Month < startDate.Month ||
            (endDate.Month == startDate.Month && endDate.Day < startDate.Day))
        {
            years--;
        }

        return years;
    }

    public static DateTime ConvertUtcToTimeZone(DateTime utcTime, string timeZoneId = "SE Asia Standard Time")
    {
        if (utcTime.Kind != DateTimeKind.Utc)
        {
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
        }

        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, targetTimeZone);
    }
}