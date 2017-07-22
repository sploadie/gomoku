# **************************************************************************** #
#                                                                              #
#                                                         :::      ::::::::    #
#    Makefile                                           :+:      :+:    :+:    #
#                                                     +:+ +:+         +:+      #
#    By: tgauvrit <tgauvrit@student.42.fr>          +#+  +:+       +#+         #
#                                                 +#+#+#+#+#+   +#+            #
#    Created: 2014/11/20 16:35:37 by tgauvrit          #+#    #+#              #
#    Updated: 2017/01/31 13:07:05 by tgauvrit         ###   ########.fr        #
#                                                                              #
# **************************************************************************** #

NAME =			gomoku_ai

CC =			clang

FLAGS =			-Wall -Werror -Wextra -Ofast

HEADERS =		-I ./include

SRC_DIR =		./src/

OBJ_DIR_NAME =	obj
OBJ_DIR =		./$(OBJ_DIR_NAME)/

FILENAMES :=	main		\
				memdup		\
				print_board	\
				read_board	\
				alphabeta   \
				heuristic	\
				player		\
				board		\
				move

OBJ_PATHS :=	$(addsuffix .o, $(FILENAMES))
OBJ_PATHS :=	$(addprefix $(OBJ_DIR),$(OBJ_PATHS))

all: $(NAME)

$(NAME): $(OBJ_PATHS)
	$(CC) $(FLAGS) $(HEADERS) $(LIBRARIES) $(OBJ_PATHS) -o $(NAME)

$(OBJ_PATHS): $(OBJ_DIR)%.o: $(SRC_DIR)%.c
	@/bin/mkdir -p $(OBJ_DIR)
	$(CC) -c $(FLAGS) $(HEADERS) $< -o $@

clean:
	-/bin/rm -f $(OBJ_PATHS)
	/usr/bin/find . -name "$(OBJ_DIR_NAME)" -maxdepth 1 -type d -empty -delete

fclean: clean
	-/bin/rm -f  $(NAME)

re: fclean all
