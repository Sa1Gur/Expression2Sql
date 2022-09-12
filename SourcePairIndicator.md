
на любой из двух источников в паре
```
hasIndicator("PairIndicator1","grey")
```

порождает Lambda выражение
```
x.PairIndicator1Items.Any(z=>z == PairIndicator1Rules.grey)
```

которое, в свою очередь приводит к sql запросу
```
SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM unifiedschedule._0862b7ab73434b6f83ab4efa73920847_1_pair_connections AS _
        INNER JOIN (
            SELECT _0.indicator_id, _0.indicator_rule_id, _0.link, _0.source_pair_id
            FROM unifiedschedule._0862b7ab73434b6f83ab4efa73920847_1_pair_collisions AS _0
            WHERE (_0.source_pair_id = '00000000-2001-0000-0000-000000000000') AND (_0.indicator_id = '00000000-3007-0000-0000-000000000000')
        ) AS t ON _.link = t.link
        WHERE (((_.source_pair_id = '00000000-2001-0000-0000-000000000000')
            AND (_.source_id = '00000000-1001-0000-0000-000000000000'))
            AND (_.row_id = _1.id))
            AND (t.indicator_rule_id = '00000000-3007-1001-0000-000000000000'))
    THEN '+'
    ELSE '-'
END
FROM unifiedschedule._0862b7ab73434b6f83ab4efa73920847_rd_2_1 AS _1
```
