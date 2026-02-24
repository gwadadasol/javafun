Feature: Pipeline Exit Codes
  As a GitHub Actions workflow
  I want the pipeline to map its result to a Unix exit code
  So that CI/CD can detect failures and partial runs correctly

  Scenario Outline: Pipeline result maps to expected exit code
    Given the pipeline result is <result>
    When the exit code is determined
    Then the exit code is <code>

    Examples:
      | result        | code |
      | Success       | 0    |
      | PartialSuccess| 1    |
      | Failure       | 2    |

