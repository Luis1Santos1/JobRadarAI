using System.Text;
using JobRadar.Domain.JobPosts;
using JobRadar.Domain.Profiles;

namespace JobRadar.Infrastructure.AI.Prompts;

public static class JobAnalysisPromptCatalog
{
    public const string JobAnalysisV1 = "job-analysis-v1";

    public static string BuildJobAnalysisPrompt(JobPost jobPost, DeveloperProfile? profile)
    {
        var profileTechnologies = profile is null
            ? "[]"
            : string.Join(", ", profile.Technologies.Select(item => item.Technology.Name));

        var builder = new StringBuilder();

        builder.AppendLine("Você é um analisador de vagas para desenvolvedores.");
        builder.AppendLine("Sua tarefa é analisar o texto colado pelo usuário e retornar APENAS um JSON válido.");
        builder.AppendLine("Não use markdown. Não explique nada fora do JSON.");
        builder.AppendLine();
        builder.AppendLine("Objetivos:");
        builder.AppendLine("1. Identificar se o texto é realmente uma vaga de emprego.");
        builder.AppendLine("2. Extrair cargo, empresa, senioridade, modalidade, tipo de contrato, localização e tecnologias.");
        builder.AppendLine("3. Avaliar aderência ao perfil do usuário.");
        builder.AppendLine("4. Retornar uma análise estruturada.");
        builder.AppendLine();
        builder.AppendLine("Perfil do usuário:");
        builder.AppendLine($"- Cargo alvo: {profile?.TargetTitle}");
        builder.AppendLine($"- Senioridade alvo: {profile?.TargetSeniority}");
        builder.AppendLine($"- Resumo: {profile?.ProfessionalSummary}");
        builder.AppendLine($"- Modelo desejado: {profile?.DesiredWorkModel}");
        builder.AppendLine($"- Contrato desejado: {profile?.DesiredContractType}");
        builder.AppendLine($"- Localizações desejadas: {profile?.DesiredLocations}");
        builder.AppendLine($"- Palavras positivas: {profile?.PositiveKeywords}");
        builder.AppendLine($"- Palavras negativas: {profile?.NegativeKeywords}");
        builder.AppendLine($"- Tecnologias do perfil: {profileTechnologies}");
        builder.AppendLine();
        builder.AppendLine("Dados cadastrados pelo usuário:");
        builder.AppendLine($"- Título informado: {jobPost.Title}");
        builder.AppendLine($"- Empresa informada: {jobPost.Company}");
        builder.AppendLine($"- Localização informada: {jobPost.Location}");
        builder.AppendLine($"- Link de origem: {jobPost.SourceUrl}");
        builder.AppendLine();
        builder.AppendLine("Texto original do post:");
        builder.AppendLine(jobPost.OriginalText);
        builder.AppendLine();
        builder.AppendLine("Retorne exatamente neste formato JSON:");
        builder.AppendLine("""
        {
          "isJobPost": true,
          "detectedTitle": "string",
          "detectedCompany": "string",
          "seniority": "Junior|Pleno|Senior|Especialista|TechLead|Unknown",
          "workModel": "Remote|Hybrid|Onsite|Unknown",
          "contractType": "CLT|PJ|Contract|Freelance|Internship|Unknown",
          "location": "string",
          "requiredTechnologies": ["string"],
          "niceToHaveTechnologies": ["string"],
          "responsibilities": ["string"],
          "requirements": ["string"],
          "benefits": ["string"],
          "redFlags": ["string"],
          "fitReasons": ["string"],
          "concerns": ["string"],
          "summary": "string",
          "aiFitScore": 0,
          "confidenceScore": 0.0,
          "recommendation": "StrongFit|GoodFit|Maybe|LowFit|NotAJobPost"
        }
        """);
        builder.AppendLine();
        builder.AppendLine("Regras:");
        builder.AppendLine("- aiFitScore deve ser inteiro de 0 a 100.");
        builder.AppendLine("- confidenceScore deve ser decimal de 0 a 1.");
        builder.AppendLine("- Se não for vaga, use isJobPost=false, recommendation=NotAJobPost e aiFitScore=0.");
        builder.AppendLine("- Se alguma informação não existir, use string vazia ou array vazio.");
        builder.AppendLine("- Não invente benefícios, contrato ou salário se não estiverem no texto.");

        return builder.ToString();
    }
}