# CHANGELOG

## 0.4.0

- Configuração da classe `AccountOptions` para realizar Logout automaticamente, sem a necessidade de clicar no link para redirecionar de volta para a aplicação cliente.

## 0.3.0

- Integração dos Clientes `MVC` e `JavaScript` com a API de Recursos (protegida). 
- Adição do scope "address", assim o cliente `MVC` pode ler o endereço das Claims do Usuário logado.
- Adição da seção `GetUserInfo` no cliente `MVC` para realizar a chamada manual do endpoint `GetUserInfo` e retornar todas as clains do usuário logado para a página.

## 0.2.0

- Adição do cliente `taskmvc` na classe `Config.js` do projeto `IdentityProvider`. Configuração exclusiva para o cliente `MvcApp`, utilizando o tipo de credencial `HybridAndClientCredentials`
- Configuração do cliente `MvcApp` para ser autenticado através do `IdentityProvider`

## 0.1.0

- Adição do projeto `Project.IdentityProvider.Api` e configuração do IdentityServer no projeto
- Adição do projeto `Project.Resources.Api`
- Adição do projeto `Project.MvcApp`