# Instruções de integração — AutoFlow.API

## 1. Copiar os arquivos

```
backend-updates/Models/Cliente.cs          → AutoFlow.API/Models/
backend-updates/Models/Veiculo.cs          → AutoFlow.API/Models/
backend-updates/DTO/ClienteDTO.cs          → AutoFlow.API/DTO/
backend-updates/DTO/FinanceiroDTO.cs       → AutoFlow.API/DTO/
backend-updates/DTO/RelatoriosDTO.cs       → AutoFlow.API/DTO/
backend-updates/Services/ClienteService.cs      → AutoFlow.API/Services/
backend-updates/Services/FinanceiroService.cs   → AutoFlow.API/Services/
backend-updates/Services/RelatoriosService.cs   → AutoFlow.API/Services/
backend-updates/Controllers/ClientesController.cs   → AutoFlow.API/Controllers/
backend-updates/Controllers/FinanceiroController.cs → AutoFlow.API/Controllers/
backend-updates/Controllers/RelatoriosController.cs → AutoFlow.API/Controllers/
```

---

## 2. Atualizar AppDbContext.cs

Adicione as duas linhas abaixo dentro da classe `AppDbContext`:

```csharp
public DbSet<Cliente> Clientes => Set<Cliente>();
public DbSet<Veiculo> Veiculos => Set<Veiculo>();
```

---

## 3. Registrar os serviços em Program.cs

Adicione as 3 linhas junto aos outros `builder.Services.AddScoped`:

```csharp
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<FinanceiroService>();
builder.Services.AddScoped<RelatoriosService>();
```

---

## 4. Gerar e aplicar a migration

```bash
dotnet ef migrations add AddClientesVeiculos
dotnet ef database update
```

---

## 5. Verificar o enum StatusOrdemServico

Os services usam `StatusOrdemServico.Pendente`, `.EmAndamento`, `.Finalizado`, `.Entregue`.
Confirme que o enum no arquivo `Models/StatusOrdemServico.cs` tem exatamente esses nomes.
Se usar nomes diferentes (ex: `Aberta` em vez de `Pendente`), ajuste os services.

---

## 6. Verificar propriedades de OrdemServico

Os services assumem que `OrdemServico` tem estas propriedades:

| Propriedade     | Tipo                   |
|-----------------|------------------------|
| `Cliente`       | `string`               |
| `Servico`       | `string`               |
| `Valor`         | `decimal`              |
| `Status`        | `StatusOrdemServico`   |
| `Faturado`      | `bool`                 |
| `DataFaturamento` | `DateTime?`          |

Se algum nome estiver diferente, ajuste as queries nos services.
