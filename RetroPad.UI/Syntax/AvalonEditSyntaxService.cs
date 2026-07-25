namespace RetroPad.UI.Syntax;
 
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using RetroPad.Core.Ports;
using System.IO;
using System.Xml;

public class AvalonEditSyntaxService
{
    private readonly Dictionary<string, IHighlightingDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISyntaxHighlighter _languageDetector;

    public AvalonEditSyntaxService(ISyntaxHighlighter languageDetector)
    {
        _languageDetector = languageDetector;
        RegisterBuiltIn();
        RegisterCustom();
    }

    public IHighlightingDefinition? GetDefinition(string language)
    {
        return _definitions.GetValueOrDefault(language);
    }

    public IHighlightingDefinition? DetectFromFile(string filePath)
    {
        var lang = _languageDetector.DetectLanguage(filePath);
        return GetDefinition(lang);
    }

    public IReadOnlyList<string> GetLanguages() => _definitions.Keys.ToList();

    private void RegisterBuiltIn()
    {
        RegisterIfAvailable("C#", "C#");
        RegisterIfAvailable("C++", "C++");
        RegisterIfAvailable("C", "C");
        RegisterIfAvailable("Java", "Java");
        RegisterIfAvailable("JavaScript", "JavaScript");
        RegisterIfAvailable("TypeScript", "TypeScript");
        RegisterIfAvailable("Python", "Python");
        RegisterIfAvailable("PHP", "PHP");
        RegisterIfAvailable("HTML", "HTML");
        RegisterIfAvailable("XML", "XML");
        RegisterIfAvailable("CSS", "CSS");
        RegisterIfAvailable("SQL", "SQL");
        RegisterIfAvailable("Markdown", "MarkDown");
        RegisterIfAvailable("Go", "Go");
    }

    private void RegisterIfAvailable(string language, string avalonName)
    {
        var def = HighlightingManager.Instance.GetDefinition(avalonName);
        if (def is not null)
            _definitions[language] = def;
    }

    private void RegisterCustom()
    {
        RegisterXshd("JSON", JsonXshd);
        RegisterXshd("YAML", YamlXshd);
        RegisterXshd("INI", IniXshd);
        RegisterXshd("Bash", BashXshd);
        RegisterXshd("PowerShell", PowerShellXshd);
        RegisterXshd("Rust", RustXshd);
        RegisterXshd("Dockerfile", DockerfileXshd);
        RegisterXshd("PlainText", PlainTextXshd);
    }

    private void RegisterXshd(string language, string xshdContent)
    {
        try
        {
            using var reader = XmlReader.Create(new StringReader(xshdContent));
            var def = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            _definitions[language] = def;
            HighlightingManager.Instance.RegisterHighlighting(language, Array.Empty<string>(), def);
        }
        catch
        {
            // Skip invalid definitions
        }
    }

    private const string PlainTextXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""PlainText"" extensions="".txt"">
  <RuleSet>
  </RuleSet>
</SyntaxDefinition>";

    private const string JsonXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""JSON"" extensions="".json"">
  <RuleSet>
    <Span color=""String"">
      <Begin>""</Begin>
      <End>""</End>
      <RuleSet>
        <Span begin=""\\"" end=""."" />
      </RuleSet>
    </Span>
    <Keywords color=""Keyword"">
      <Word>true</Word>
      <Word>false</Word>
      <Word>null</Word>
    </Keywords>
    <Rule color=""Number"">-?\d+\.?\d*([eE][+-]?\d+)?</Rule>
    <Rule color=""Comment"">//.*$</Rule>
  </RuleSet>
</SyntaxDefinition>";

    private const string YamlXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""YAML"" extensions="".yaml;.yml"">
  <RuleSet>
    <Span color=""Comment"" begin=""#"" end=""$"" />
    <Span color=""String"">
      <Begin>""</Begin>
      <End>""</End>
    </Span>
    <Span color=""String"">
      <Begin>'</Begin>
      <End>'</End>
    </Span>
    <Rule color=""Keyword"">^[\w.-]+:</Rule>
    <Rule color=""Number"">-?\d+\.?\d*</Rule>
    <Keywords color=""Keyword"">
      <Word>true</Word>
      <Word>false</Word>
      <Word>null</Word>
      <Word>yes</Word>
      <Word>no</Word>
    </Keywords>
  </RuleSet>
</SyntaxDefinition>";

    private const string IniXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""INI"" extensions="".ini;.cfg;.conf"">
  <RuleSet>
    <Span color=""Comment"" begin=""#"" end=""$"" />
    <Span color=""Comment"" begin="";"" end=""$"" />
    <Rule color=""Keyword"">^\[[\w\s.-]+\]</Rule>
    <Rule color=""Type"">^[\w.-]+(?=\s*=)</Rule>
    <Span color=""String"">
      <Begin>""</Begin>
      <End>""</End>
    </Span>
    <Rule color=""Number"">\d+\.?\d*</Rule>
  </RuleSet>
</SyntaxDefinition>";

    private const string BashXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Bash"" extensions="".sh;.bash"">
  <RuleSet>
    <Span color=""Comment"" begin=""#"" end=""$"" />
    <Span color=""String"">
      <Begin>""</Begin>
      <End>""</End>
      <RuleSet>
        <Span begin=""\\"" end=""."" />
        <Rule color=""Keyword"">\$[\w{]+[\w}]?</Rule>
      </RuleSet>
    </Span>
    <Span color=""String"">
      <Begin>'</Begin>
      <End>'</End>
    </Span>
    <Keywords color=""Keyword"">
      <Word>if</Word><Word>then</Word><Word>else</Word><Word>elif</Word>
      <Word>fi</Word><Word>for</Word><Word>while</Word><Word>do</Word>
      <Word>done</Word><Word>case</Word><Word>esac</Word><Word>function</Word>
      <Word>return</Word><Word>local</Word><Word>export</Word><Word>readonly</Word>
      <Word>declare</Word><Word>set</Word><Word>unset</Word><Word>shift</Word>
      <Word>in</Word><Word>echo</Word><Word>exit</Word><Word>source</Word>
      <Word>cd</Word><Word>eval</Word><Word>exec</Word><Word>trap</Word>
    </Keywords>
    <Rule color=""Method"">\b[\w-]+(?=\s*\()</Rule>
    <Rule color=""Number"">\b\d+\b</Rule>
  </RuleSet>
</SyntaxDefinition>";

    private const string PowerShellXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""PowerShell"" extensions="".ps1;.psm1;.psd1"">
  <RuleSet>
    <Span color=""Comment"" begin=""#"" end=""$"" />
    <Span color=""Comment"">
      <Begin>&lt;#</Begin>
      <End>#&gt;</End>
    </Span>
    <Span color=""String"">
      <Begin>""</Begin>
      <End>""</End>
      <RuleSet>
        <Rule color=""Keyword"">\$[\w{]+[\w}]?</Rule>
      </RuleSet>
    </Span>
    <Span color=""String"">
      <Begin>'</Begin>
      <End>'</End>
    </Span>
    <Keywords color=""Keyword"">
      <Word>function</Word><Word>filter</Word><Word>workflow</Word><Word>class</Word>
      <Word>enum</Word><Word>param</Word><Word>begin</Word><Word>process</Word>
      <Word>end</Word><Word>if</Word><Word>elseif</Word><Word>else</Word>
      <Word>switch</Word><Word>for</Word><Word>foreach</Word><Word>while</Word>
      <Word>do</Word><Word>until</Word><Word>return</Word><Word>break</Word>
      <Word>continue</Word><Word>throw</Word><Word>try</Word><Word>catch</Word>
      <Word>finally</Word><Word>trap</Word><Word>exit</Word><Word>in</Word>
      <Word>var</Word>
    </Keywords>
    <Rule color=""Method"">\b[\w-]+(?=\s*\()</Rule>
    <Rule color=""Number"">\b\d+\.?\d*\b</Rule>
  </RuleSet>
</SyntaxDefinition>";

    private const string RustXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Rust"" extensions="".rs"">
  <RuleSet>
    <Span color=""Comment"" begin=""//"" end=""$"" />
    <Span color=""Comment"">
      <Begin>/*</Begin>
      <End>*/</End>
    </Span>
    <Span color=""String"">
      <Begin>""</Begin>
      <End>""</End>
      <RuleSet>
        <Span begin=""\\"" end=""."" />
      </RuleSet>
    </Span>
    <Keywords color=""Keyword"">
      <Word>as</Word><Word>break</Word><Word>const</Word><Word>continue</Word>
      <Word>crate</Word><Word>else</Word><Word>enum</Word><Word>extern</Word>
      <Word>false</Word><Word>fn</Word><Word>for</Word><Word>if</Word>
      <Word>impl</Word><Word>in</Word><Word>let</Word><Word>loop</Word>
      <Word>match</Word><Word>mod</Word><Word>move</Word><Word>mut</Word>
      <Word>pub</Word><Word>ref</Word><Word>return</Word><Word>self</Word>
      <Word>static</Word><Word>struct</Word><Word>super</Word><Word>trait</Word>
      <Word>true</Word><Word>type</Word><Word>unsafe</Word><Word>use</Word>
      <Word>where</Word><Word>while</Word><Word>async</Word><Word>await</Word>
      <Word>dyn</Word>
    </Keywords>
    <Keywords color=""Type"">
      <Word>bool</Word><Word>char</Word><Word>f32</Word><Word>f64</Word>
      <Word>i8</Word><Word>i16</Word><Word>i32</Word><Word>i64</Word><Word>i128</Word>
      <Word>isize</Word><Word>str</Word><Word>u8</Word><Word>u16</Word>
      <Word>u32</Word><Word>u64</Word><Word>u128</Word><Word>usize</Word>
      <Word>String</Word><Word>Vec</Word><Word>Option</Word><Word>Result</Word>
    </Keywords>
    <Rule color=""Method"">\b[\w]+(?=\s*\()</Rule>
    <Rule color=""Number"">\b\d+\.?\d*(_\d+)*\b</Rule>
  </RuleSet>
</SyntaxDefinition>";

    private const string DockerfileXshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Dockerfile"" extensions=""Dockerfile"">
  <RuleSet>
    <Span color=""Comment"" begin=""#"" end=""$"" />
    <Span color=""String"">
      <Begin>""</Begin>
      <End>""</End>
    </Span>
    <Span color=""String"">
      <Begin>'</Begin>
      <End>'</End>
    </Span>
    <Keywords color=""Keyword"">
      <Word>FROM</Word><Word>RUN</Word><Word>CMD</Word><Word>EXPOSE</Word>
      <Word>ENV</Word><Word>ADD</Word><Word>COPY</Word><Word>ENTRYPOINT</Word>
      <Word>VOLUME</Word><Word>USER</Word><Word>WORKDIR</Word><Word>ARG</Word>
      <Word>ONBUILD</Word><Word>STOPSIGNAL</Word><Word>HEALTHCHECK</Word><Word>SHELL</Word>
      <Word>LABEL</Word><Word>MAINTAINER</Word>
    </Keywords>
    <Rule color=""Number"">\b\d+\b</Rule>
  </RuleSet>
</SyntaxDefinition>";
}
