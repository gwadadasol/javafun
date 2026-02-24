Feature: DSCR Property Qualification
  As a real estate investor
  I want properties to be automatically qualified based on their DSCR ratio
  So that I can identify the best investment opportunities

  Background:
    Given the default DSCR thresholds

  Scenario Outline: Property qualification based on price and rent
    Given a property priced at <price>
    And the HUD FMR monthly rent is <rent>
    When the DSCR analysis is performed
    Then the property DSCR status is <status>

    Examples:
      | price   | rent | status    |
      | 150000  | 2000 | Qualified |
      | 200000  | 2000 | Warning   |
      | 500000  | 1000 | Rejected  |

  Scenario: Property with unavailable FMR data is rejected
    Given a property priced at 200000
    And FMR rent is zero
    When the DSCR analysis is performed
    Then the property DSCR status is Rejected

  Scenario: DSCR ratio is rounded to 4 decimal places
    Given a property priced at 200000
    And the HUD FMR monthly rent is 1500
    When the DSCR analysis is performed
    Then the DSCR ratio has at most 4 decimal places

  Scenario: No loan means property is rejected
    Given a property priced at 200000
    And the down payment is 100 percent
    And the HUD FMR monthly rent is 2000
    When the DSCR analysis is performed
    Then the property DSCR status is Rejected

