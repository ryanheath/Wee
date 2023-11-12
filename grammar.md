$$
\begin{align}
    [\text{Program}] &\to [\text{Statements}] \\
    \\
    [\text{Statements}] &\to [\text{Statement}]^* \\
    \\
    [\text{Statement}] &\to
        \begin{cases}
            \text{return}\ [\text{Expression}]^?; \\
            \text{let}\space\text{[identifier]} = [\text{Expression}]; \\
            \text{print}\ [\text{Expression}]^?; \\
        \end{cases} \\
        \\
    [\text{Expression}] &\to
        \begin{cases}
            [\text{Term}] \\
            [\text{BinaryExpression}] \\
        \end{cases} \\
        \\
    [\text{BinaryExpression}] &\to
        \begin{cases}
            [\text{Expression}] * [\text{Expression}]\\
            [\text{Expression}]\ /\ [\text{Expression}]\\
            [\text{Expression}] + [\text{Expression}]\\
            [\text{Expression}] - [\text{Expression}]\\
        \end{cases} \\
        \\
    [\text{LogicalExpression}] &\to
        \begin{cases}
            [\text{Expression}]\ \text{or}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{and}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{eq}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{lt}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{lte}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{gt}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{gte}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{neq}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{||}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{\&\&}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{=}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{<}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{<=}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{>}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{>=}\ [\text{Expression}]\\
            [\text{Expression}]\ \text{!=}\ [\text{Expression}]\\
        \end{cases} \\
        \\
    [\text{Term}] &\to
        \begin{cases}
            [\text{number}] \\
            [\text{string}] \\
            [\text{identifier}] \\
            [\text{boolean}] \\
        \end{cases} \\
        \\
    [\text{boolean}] &\to
        \begin{cases}
            \text{true} \\
            \text{false} \\
        \end{cases} \\
        \\
    [\text{number}] &\to \text[0-9]+\text[0-9]^* \\
    \\
    [\text{string}] &\to
        \begin{cases}
            \text{"}+[.]^*+\text{"} \\
            \text{'}+[.]^*+\text{'} \\
        \end{cases} \\
        \\
    [\text{identifier}] &\to \text[A-Za-z]+\text[A-Za-z0-9]^* \\
\end{align}
$$