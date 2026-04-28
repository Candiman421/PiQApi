// CertApi.ArchitectureRules/Solution/README.md
# CertApi Architecture Documentation

This project contains architectural guidelines, enforcement tests, and validation tools for the CertApi solution.

## Organization

- **/Solution**: Solution-wide architectural guidelines and standards
  - `/Diagrams`: PlantUML diagrams defining solution structure
  - `/Guides`: Architectural principle documentation
  - `/Enforcement`: Architectural validation tests
  - `/Fixit`: Issue tracking and resolution plans

- **/Projects**: Project-specific architectural guidelines
  - Individual project folders containing:
    - Project-specific diagrams
    - Implementation guides
    - Validation rules
    - Issue tracking

## Guide Standards

1. **Naming Convention**:
   - Solution guides: `CertApi.Solution_Category_Name.json`
   - Project guides: `CertApi.ProjectName_GuideName.json`
   - Cross-cutting: `CertApi.Solution_CrossCutting_Name.json`

2. **Required Metadata**:
   - version
   - last_updated
   - scope
   - dependencies
   - applies_to

3. **Content Sections**:
   - principles
   - implementation
   - patterns
   - forbidden_patterns
   - examples

## Status Tracking

Guide status is tracked in `/Solution/Fixit/guide_status.json`:
- NotStarted: Initial state
- InProgress: Under review/modification
- Consolidated: Merged or updated
- Verified: Final state

## Issue Tracking

Issues are documented using codes defined in `/Solution/Fixit/IssuesLegend.json`

## Usage

This documentation serves as the source of truth for:
1. Architectural principles and patterns
2. Implementation standards
3. Project organization rules
4. Cross-cutting concerns