Generate a security and privacy review for the application based on the technical specification that has been provided.  The security and privacy review should only consider the production environment.

Use the following format:

Use the following format to generate your spec:

```markdown
# Security and Privacy Review

## 1. Security Review
- Decompose the application into relevant components and provide a block diagram showing the data flows between the components.  Identify trust zones within the diagram.
- Provide a threat model and use "STRIDE per element" to discuss threats to the application.
- Discuss the encryption technologies used and how they are used.
- Discuss how services will authenticate and communicate security with each other.
- Discuss how the application handles the OWASP top-10 vulnerabilities.
- Discuss auditing requirements that may be required to meet security goals.
- Identify third-party components and their risks in the application.
- Identify any security flaws in the design of the technical specification.
- Suggest mitigations for any security flaws found.

## 2. Privacy Review
- Decompose the data model for the application into relevant components and provide PII classification.
- Discuss how the application meets or exceeds the requirements of GDPR, UK-DPA, and other common privacy regulations.
- Identify regional data boundaries within the privacy model.
- Identify special processing considerations that may be region specific.
- Provide any data controls, consent mechanisms, and cookie management.
```

Please:
1. Ask me questions about any areas that need more detail
2. Suggest considerations related to security and privacy I might have missed
3. Help me organize requirements logically
4. Show me the current state of the spec after each exchange
5. Flag any potential technical challenges or important decisions
