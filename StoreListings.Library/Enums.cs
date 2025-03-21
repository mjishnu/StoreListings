﻿namespace StoreListings.Library;

/// <summary>
/// Represents categories used for recommendations.
/// </summary>
public enum Category
{
    TopFree,
    TopPaid,
    BestRated,
    Deal,
    NewAndRising,
    TopGrossing,
    Mostpopular,
}

/// <summary>
/// Defines media types that can be specified when searching.
/// </summary>
public enum MediaTypeSearch
{
    all,
    apps,
    games,
    devices,
    passes,
    fonts,
    themes,
}

/// <summary>
/// Defines media types that can be specified when requesting recommendations.
/// </summary>
public enum MediaTypeRecommendation
{
    all,
    apps,
    games,
}

/// <summary>
/// Specifies price filtering options for search queries.
/// </summary>
public enum PriceType
{
    All,
    Free,
    Paid,
    Sale,
}

/// <summary>
/// Represents a Windows device family.
/// </summary>
public enum DeviceFamily
{
    /// <summary>
    /// Unknown device family
    /// </summary>
    Unknown,

    /// <summary>
    /// Windows IoT
    /// </summary>
    /// <remarks>Only seen in StoreLib?</remarks>
    Iot,

    /// <summary>
    /// Windows IoT
    /// </summary>
    /// <remarks>Only seen in UUPMediaCreator?</remarks>
    IoTUAP,

    /// <summary>
    /// Windows Server
    /// </summary>
    Server,

    /// <summary>
    /// Windows Team (Surface Hub 2S and earlier)
    /// </summary>
    Team,

    /// <summary>
    /// Windows Holographic (HoloLens)
    /// </summary>
    Holographic,

    /// <summary>
    /// Windows 10 Mobile
    /// </summary>
    Mobile,

    /// <summary>
    /// Windows Core OS
    /// </summary>
    Core,

    /// <summary>
    /// Xbox
    /// </summary>
    Xbox,

    /// <summary>
    /// Windows Desktop
    /// </summary>
    Desktop,

    /// <summary>
    /// Universal (all device families)
    /// </summary>
    Universal,

    // StoreLib also suggests 8828080 for Andromeda, but UUPMediaCreator says that Andromeda uses Core?
    // Andromeda is dead anyways so I guess it won't hurt leaving it out.
}

/// <summary>
/// Represents an installer type.
/// </summary>
public enum InstallerType
{
    /// <summary>
    /// A packaged (APPX/MSIX/MSIXVC) installer.
    /// </summary>
    Packaged,

    /// <summary>
    /// An unpackaged (MSI/EXE) installer.
    /// </summary>
    Unpackaged,

    /// <summary>
    /// An unknown installer type.
    /// </summary>
    Unknown,
}

/// <summary>
/// Represents a Microsoft Store market.
/// </summary>
public enum Market
{
    US,
    DZ,
    AR,
    AU,
    AT,
    BH,
    BD,
    BE,
    BR,
    BG,
    CA,
    CL,
    CN,
    CO,
    CR,
    HR,
    CY,
    CZ,
    DK,
    EG,
    EE,
    FI,
    FR,
    DE,
    GR,
    GT,
    HK,
    HU,
    IS,
    IN,
    ID,
    IQ,
    IE,
    IL,
    IT,
    JP,
    JO,
    KZ,
    KE,
    KW,
    LV,
    LB,
    LI,
    LT,
    LU,
    MY,
    MT,
    MR,
    MX,
    MA,
    NL,
    NZ,
    NG,
    NO,
    OM,
    PK,
    PE,
    PH,
    PL,
    PT,
    QA,
    RO,
    RU,
    SA,
    RS,
    SG,
    SK,
    SI,
    ZA,
    KR,
    ES,
    SE,
    CH,
    TW,
    TH,
    TT,
    TN,
    TR,
    UA,
    AE,
    GB,
    VN,
    YE,
    LY,
    LK,
    UY,
    VE,
    AF,
    AX,
    AL,
    AS,
    AO,
    AI,
    AQ,
    AG,
    AM,
    AW,
    BO,
    BQ,
    BA,
    BW,
    BV,
    IO,
    BN,
    BF,
    BI,
    KH,
    CM,
    CV,
    KY,
    CF,
    TD,
    TL,
    DJ,
    DM,
    DO,
    EC,
    SV,
    GQ,
    ER,
    ET,
    FK,
    FO,
    FJ,
    GF,
    PF,
    TF,
    GA,
    GM,
    GE,
    GH,
    GI,
    GL,
    GD,
    GP,
    GU,
    GG,
    GN,
    GW,
    GY,
    HT,
    HM,
    HN,
    AZ,
    BS,
    BB,
    BY,
    BZ,
    BJ,
    BM,
    BT,
    KM,
    CG,
    CD,
    CK,
    CX,
    CC,
    CI,
    CW,
    JM,
    SJ,
    JE,
    KI,
    KG,
    LA,
    LS,
    LR,
    MO,
    MK,
    MG,
    MW,
    IM,
    MH,
    MQ,
    MU,
    YT,
    FM,
    MD,
    MN,
    MS,
    MZ,
    MM,
    NA,
    NR,
    NP,
    MV,
    ML,
    NC,
    NI,
    NE,
    NU,
    NF,
    PW,
    PS,
    PA,
    PG,
    PY,
    RE,
    RW,
    BL,
    MF,
    WS,
    ST,
    SN,
    MP,
    PN,
    SX,
    SB,
    SO,
    SC,
    SL,
    GS,
    SH,
    KN,
    LC,
    PM,
    VC,
    TJ,
    TZ,
    TG,
    TK,
    TO,
    TM,
    TC,
    TV,
    UM,
    UG,
    VI,
    VG,
    WF,
    EH,
    ZM,
    ZW,
    UZ,
    VU,
    SR,
    SZ,
    AD,
    MC,
    SM,
    ME,
    VA,
}

/// <summary>
/// Represents a language.
/// </summary>
public enum Lang
{
    iv,
    aa,
    af,
    agq,
    ak,
    am,
    ar,
    arn,
    asa,
    ast,
    az,
    ba,
    bas,
    be,
    bem,
    bez,
    bg,
    bin,
    bm,
    bn,
    bo,
    br,
    brx,
    bs,
    byn,
    ca,
    ce,
    cgg,
    chr,
    co,
    cs,
    cu,
    cy,
    da,
    dav,
    de,
    dje,
    dsb,
    dua,
    dv,
    dyo,
    dz,
    ebu,
    ee,
    el,
    en,
    eo,
    es,
    et,
    eu,
    ewo,
    fa,
    ff,
    fi,
    fil,
    fo,
    fr,
    fur,
    fy,
    ga,
    gd,
    gl,
    gn,
    gsw,
    gu,
    guz,
    gv,
    ha,
    haw,
    he,
    hi,
    hr,
    hsb,
    hu,
    hy,
    ia,
    ibb,
    id,
    ig,
    ii,
    it,
    iu,
    ja,
    jgo,
    jmc,
    jv,
    ka,
    kab,
    kam,
    kde,
    kea,
    khq,
    ki,
    kk,
    kkj,
    kl,
    kln,
    km,
    kn,
    ko,
    kok,
    kr,
    ks,
    ksb,
    ksf,
    ksh,
    ku,
    kw,
    ky,
    la,
    lag,
    lb,
    lg,
    lkt,
    ln,
    lo,
    lrc,
    lt,
    lu,
    luo,
    luy,
    lv,
    mas,
    mer,
    mfe,
    mg,
    mgh,
    mgo,
    mi,
    mk,
    ml,
    mn,
    mni,
    moh,
    mr,
    ms,
    mt,
    mua,
    my,
    mzn,
    naq,
    nb,
    nd,
    ne,
    nl,
    nmg,
    nn,
    nnh,
    nqo,
    nr,
    nso,
    nus,
    nyn,
    oc,
    om,
    or,
    os,
    pa,
    pap,
    pl,
    prg,
    prs,
    ps,
    pt,
    quc,
    quz,
    rm,
    rn,
    ro,
    rof,
    ru,
    rw,
    rwk,
    sa,
    sah,
    saq,
    sbp,
    sd,
    se,
    seh,
    ses,
    sg,
    shi,
    si,
    sk,
    sl,
    sma,
    smj,
    smn,
    sms,
    sn,
    so,
    sq,
    sr,
    ss,
    ssy,
    st,
    sv,
    sw,
    syr,
    ta,
    te,
    teo,
    tg,
    th,
    ti,
    tig,
    tk,
    tn,
    to,
    tr,
    ts,
    tt,
    twq,
    tzm,
    ug,
    uk,
    ur,
    uz,
    vai,
    ve,
    vi,
    vo,
    vun,
    wae,
    wal,
    wo,
    xh,
    xog,
    yav,
    yi,
    yo,
    zgh,
    zh,
    zu,
}
