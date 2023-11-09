$$
\begin{align}
    [\text{Program}] &\to [\text{Statements}] \\
    [\text{Statements}] &\to [\text{Statement}]^* \\
    [\text{Statement}] &\to
        \begin{cases}
            \text{return}\ [\text{Expression}]^?; \\
            \text{let}\space\text{[identifier]} = [\text{Expression}]; \\
            \text{print}\ [\text{Expression}]^?; \\
        \end{cases} \\
    [\text{Expression}] &\to
        \begin{cases}
            [\text{number}] \\
            [\text{string}] \\
            [\text{identifier}] \\
        \end{cases} \\
        \\
    [\text{number}] &\to \text[0-9]+\text[0-9]^* \\
    [\text{string}] &\to
        \begin{cases}
            \text{"}+[.]^*+\text{"} \\
            \text{'}+[.]^*+\text{'} \\
        \end{cases} \\
    [\text{identifier}] &\to \text[A-Za-z]+\text[A-Za-z0-9]^* \\
\end{align}
$$