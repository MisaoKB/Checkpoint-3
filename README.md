# Checkpoint-3
Atividade da matéria de aplicações orientadas a objetos, onde se deve analisar um estudo de caso e identificar e corrigir alguns problemas.

Esses são os resultados sobre a análise do estudo de caso:

1 - Quando cadastra usuário

Problema: Além de adicionar o usuário, o método também manda e-mail de boas-vindas.

Quem cuida só de cadastro não deveria se preocupar com e-mail.

2 - Na hora de emprestar livro

Problema: O método faz o empréstimo e envia e-mail e SMS.

Mistura muitas tarefas num lugar só: cadastro, lógica de empréstimo e notificações.

3 - Devolução e multa

Problema: Esse método atualiza o livro, calcula multa e envia e-mail.

É um método grande, com lógica misturada, difícil de entender e testar.

4 - Notificações dentro da classe principal

Problema: GerenciadorBiblioteca chama diretamente EnviarEmail e EnviarSMS.

Fica preso a essas implementações, se quiser mudar pra outro tipo de notificação vai mexer nessa classe.

5 - Entidades sem comportamento claro

Problema: As classes Livro, Usuário e Empréstimo só guardam dados, mas não fazem nada (por exemplo, marcar livro como emprestado).

A lógica fica toda solta no gerenciador, em vez de cada classe cuidar do seu próprio trabalho.
