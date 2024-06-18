namespace Content.Shared.Security;

/// <summary>
/// Status used in Criminal Records.
///
/// None - the default value
/// Suspected - the person is suspected of doing something illegal
/// Wanted - the person is being wanted by security
/// Detained - the person is detained by security
/// Paroled - the person is on parole
/// Discharged - the person has been released from prison
/// # DeltaV - Subpoenaed - the person has been subpoenaed by the Justice Dept. 
/// </summary>
public enum SecurityStatus : byte
{
    None,
    Suspected,
    Wanted,
    Detained,
    Paroled,
    Discharged,
    Subpoenaed, /// # DeltaV - Justice Department Subpoena
}
