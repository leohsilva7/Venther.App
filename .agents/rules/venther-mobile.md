# Venther.App - Diretrizes de Desenvolvimento Mobile

## 1. Stack Tecnológica
- **Framework:** .NET MAUI (.NET 10) focado em Android.
- **Padrão de Arquitetura:** MVVM com `CommunityToolkit.Mvvm`.
- **Toolkit:** `CommunityToolkit.Maui`.
- **Navegação:** .NET MAUI Shell (`AppShell`).

## 2. Padrão MVVM & Injeção de Dependência
- Todas as ViewModels herdam de `BaseViewModel` (`ObservableObject`).
- Uso dos source generators do Toolkit: `[ObservableProperty]` e `[RelayCommand]`.
- ViewModels e Pages registradas como `Transient` no `MauiProgram.cs`.
- Serviços e repositórios registrados como `Singleton` no `MauiProgram.cs`.
- As Pages recebem a respectiva ViewModel via injeção de dependência no construtor e definem `BindingContext = viewModel`.

## 3. Regras de Nomenclatura e Idioma
- **Código:** 100% em inglês (classes, métodos, propriedades, variáveis, arquivos, namespaces).
- **Interface com Usuário (UI):** 100% em Português do Brasil (pt-BR).
- **Convenções:**
  - Classes e Métodos: PascalCase (`DeviceDetailViewModel`, `GetDevicesAsync`).
  - Campos privados: camelCase com prefixo `_` (`_isBusy`, `_devices`).
  - Comandos: sufixo gerado `Command` (`EntrarCommand`).

## 4. Tokens de Cor (Resources/Styles/Colors.xaml)
- `Primary`: `#00326F` (Azul institucional)
- `Accent`: `#FC6100` (Laranja de destaque/ação)
- `Background`: `#F6F9FF` (Fundo geral do app)
- `Surface`: `#EEF4FC` (Fundo de cards e superfícies elevadas)
- `SurfaceVariant`: `#E8EEF6`
- `OnSurface`: `#161C22` (Texto principal sobre Surface)
- `OnSurfaceVariant`: `#424751` (Texto secundário/muted)
- `Outline`: `#737783` (Bordas e divisores padrão)
- `AccentContainer`: `#FFDBCD`
- `OnAccentContainer`: `#360F00`
- `AccentText`: `#A43D00`
- `AuthText`: `#0F172A` (Texto escuro telas de auth)
- `AuthMuted`: `#64748B` (Texto de apoio auth)
- `AuthBorder`: `#E2E8F0` (Borda de inputs e cards de auth)
- `Error`: `#BA1A1A`
- `ErrorBg`: `#FEF2F2`
- `ErrorBorder`: `#FECACA`
- `LogoBlue`: `#034899`
- `White`: `#FFFFFF`

## 5. Tipografia (Inter)
Fontes estáticas Inter registradas via aliases:
- `InterRegular`: Fonte padrão de corpo.
- `InterMedium`: Textos com ênfase média, títulos médios e botões.
- `InterSemiBold`: Títulos fortes e overlines.
- `InterBold`: Destaques numéricos e títulos principais.
- *Fallback:* `OpenSansRegular`, `OpenSansSemibold`.

### Estilos Pré-definidos (`Styles.xaml`)
- **Padrão (`Label`):** InterRegular, 14, `OnSurface`.
- **`TitleAuth`:** InterSemiBold, 24, `AuthText`.
- **`Heading`:** InterMedium, 18.
- **`BodySmall`:** 13, `OnSurfaceVariant`.
- **`Caption`:** InterMedium, 12.
- **`Overline`:** InterSemiBold, 11, `CharacterSpacing=0.55`, `TextTransform=Uppercase`, `OnSurfaceVariant`.

## 6. Formas e Dimensões Padrão
- **Raios de Borda (CornerRadius):**
  - Pequeno / Botões e Cards padrão: `8`
  - Médio / Inputs e Diálogos: `12`
  - Grande / Auth Cards e Modais: `16`
- **Alturas Fixas:**
  - Botão / Input: `48`
  - Header: `64`
  - Tab Bar: `80`

## 7. Regra Fundamental
> **use sempre os estilos de Styles.xaml e os tokens de Colors.xaml; nunca coloque cores ou tamanhos fixos nas telas**.
- Nunca use `Frame` (obsoleto); use sempre `Border`.
- Não instale Tailwind ou bibliotecas web.
