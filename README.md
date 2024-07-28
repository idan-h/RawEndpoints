[![license](https://img.shields.io/github/license/idan-h/RawEndpoints?color=blue&label=license&logo=Github&style=flat-square)](https://github.com/idan-h/RawEndpoints/blob/main/LICENSE.md) [![nuget](https://img.shields.io/nuget/v/RawEndpoints?label=version&logo=NuGet&style=flat-square)](https://www.nuget.org/packages/RawEndpoints) [![nuget](https://img.shields.io/nuget/dt/RawEndpoints?color=blue&label=downloads&logo=NuGet&style=flat-square)](https://www.nuget.org/packages/RawEndpoints)

# RawEndpoints

RawEndpoints is a simple library for REST API development using .NET's Minimal API, adhering to the REPR (Request-Endpoint-Response) approach. It leverages source generators to simplify endpoint creation, reducing boilerplate code and providing a clean, non-opinionated solution.

## Design & Approach

- **Minimal API-Based**: Directly utilizes .NET's Minimal API, not a wrapper around it.
- **Source Generators**: Automatically generate endpoint methods to streamline development.
- **Non-Opinionated**: Use it raw, partly, or fully. You decide.
- **Focused and Clean**: Aims to be minimalistic and stable.

## Roadmap

- [ ] **Validation**: Integrate validations within the source generator.
- [ ] **Request Response OpenApi**: Enable automatic OpenAPI with request and response types.
- [ ] **Result OpenApi**: Analyze potenrial results and add them to OpenAPI specifications.
- [ ] **VS Templates**: Provide Visual Studio templates for new endpoints.
- [ ] **Benchmarks**: Add performance benchmarks.

## Feature Requests

Feature requests are appreciated :)  
Keep in mind that maintaining the library's goals of minimalism, stability, and non-opinionated design is the top priority.

## License

This project is licensed under the MIT License.

---